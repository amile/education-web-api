using System.Collections.Concurrent;
using EducationWebApi.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EducationWebApi.Tests;

public class BookingServiceTests
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IServiceScope _scope;
    private readonly IEventsService _eventsService;
    private readonly IBookingRepository _bookingRepository;
    private readonly IBookingService _bookingService;

    public BookingServiceTests()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IEventsRepository, EventsRepository>();
        services.AddScoped<IEventsService, EventsService>();
        services.AddScoped<IBookingService, BookingService>();

        _serviceProvider = services.BuildServiceProvider();
        _scope = _serviceProvider.CreateScope();
        _bookingRepository = _scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        _eventsService = _scope.ServiceProvider.GetRequiredService<IEventsService>();
        _bookingService = _scope.ServiceProvider.GetRequiredService<IBookingService>();
    }

    [Fact]
    public async Task BookEvent_Ok()
    {
        //Arrange
        var eventId = await CreateEvent("event1");

        //Act
        var actual = await _bookingService.CreateBookingAsync(eventId);

        //Assert
        Assert.NotNull(actual);
        Assert.Equal(eventId, actual.EventId);
        Assert.Equal(BookingStatus.Pending, actual.Status);
    }

    [Fact]
    public async Task BookEventMultiple_Ok()
    {
        //Arrange
        var eventId = await CreateEvent("event1", totalSeats: 3);

        //Act
        var actualBooking1 = await _bookingService.CreateBookingAsync(eventId);
        var actualBooking2 = await _bookingService.CreateBookingAsync(eventId);
        var actualBooking3 = await _bookingService.CreateBookingAsync(eventId);

        //Assert
        Assert.NotNull(actualBooking1);
        Assert.NotNull(actualBooking2);
        Assert.NotNull(actualBooking3);
        Assert.NotEqual(actualBooking1.Id, actualBooking2.Id);
        Assert.NotEqual(actualBooking1.Id, actualBooking3.Id);
        Assert.NotEqual(actualBooking2.Id, actualBooking3.Id);
    }

    [Fact]
    public async Task BookEvent_ReserveSeats_Ok()
    {
        //Arrange
        var eventId = await CreateEvent("event1", totalSeats: 3);

        //Act
        await _bookingService.CreateBookingAsync(eventId);
        var actualEvent = await _eventsService.GetEventAsync(eventId);

        //Assert
        Assert.NotNull(actualEvent);
        Assert.Equal(eventId, actualEvent.Id);
        Assert.Equal(2, actualEvent.AvailableSeats);
    }

    [Fact]
    public async Task BookEventMultiple_ReserveSeats_Ok()
    {
        //Arrange
        var eventId = await CreateEvent("event1", totalSeats: 3);

        //Act
        var actualBooking1 = await _bookingService.CreateBookingAsync(eventId);
        var actualBooking2 = await _bookingService.CreateBookingAsync(eventId);
        var actualBooking3 = await _bookingService.CreateBookingAsync(eventId);

        var actualEvent = await _eventsService.GetEventAsync(eventId);

        //Assert
        Assert.Equal(0, actualEvent.AvailableSeats);
        Assert.NotEqual(actualBooking1.Id, actualBooking2.Id);
        Assert.NotEqual(actualBooking1.Id, actualBooking3.Id);
        Assert.NotEqual(actualBooking2.Id, actualBooking3.Id);
    }

    [Fact]
    public async Task BookEvent_ReserveSeats_Error()
    {
        //Arrange
        var eventId = await CreateEvent("event1");
        await _bookingService.CreateBookingAsync(eventId);

        //Assert
        var error = await Assert.ThrowsAsync<NoAvailableSeatsException>(() => _bookingService.CreateBookingAsync(eventId));
    }

    [Fact]
    public async Task GetBooking_Ok()
    {
        //Arrange
        var eventId = await CreateEvent("event1");
        var expectedBooking = await _bookingService.CreateBookingAsync(eventId);

        //Act
        var actualBooking = await _bookingService.GetBookingByIdAsync(expectedBooking.Id);

        //Assert
        Assert.NotNull(actualBooking);
        Assert.Equal(expectedBooking.Id, actualBooking.Id);
        Assert.Equal(expectedBooking.EventId, actualBooking.EventId);
        Assert.Equal(expectedBooking.Status, actualBooking.Status);
    }

    [Fact]
    public async Task BookEvent_WrongEventId()
    {
        //Arrange
        var eventId = Guid.NewGuid();

        //Assert
        var error = await Assert.ThrowsAsync<KeyNotFoundException>(() => _bookingService.CreateBookingAsync(eventId));
        Assert.Equal($"Event Id: {eventId} not found", error.Message);
    }

    [Fact]
    public async Task BookEvent_DeletedEvent()
    {
        //Arrange
        var eventId = await CreateEvent("event1");
        await _eventsService.RemoveEventAsync(eventId);

        //Assert
        var error = await Assert.ThrowsAsync<KeyNotFoundException>(() => _bookingService.CreateBookingAsync(eventId));
        Assert.Equal($"Event Id: {eventId} not found", error.Message);
    }

    [Fact]
    public async Task GetBooking_WrongId()
    {
        //Arrange
        var eventId = await CreateEvent("event1");
        await _bookingService.CreateBookingAsync(eventId);
        var wrongId = Guid.NewGuid();

        //Assert
        var error = await Assert.ThrowsAsync<KeyNotFoundException>(() => _bookingService.GetBookingByIdAsync(wrongId));
        Assert.Equal($"Booking Id: {wrongId} not found", error.Message);
    }

    [Fact]
    public async Task BookEvent_RaceCondition()
    {
        //Arrange
        var eventId = await CreateEvent("event1", totalSeats: 5);
        var tasksCount = 20;

        //Act
        var tasks = Enumerable.Range(0, tasksCount).Select(i => Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

            try
            {
                await bookingService.CreateBookingAsync(eventId);
                return true;
            }
            catch (NoAvailableSeatsException)
            {
                return false;
            }
        }));

        var results = await Task.WhenAll(tasks);
        var completedBookings = results.Count(r => r);
        var failedBookings = tasksCount - completedBookings;

        //Assert
        Assert.Equal(5, completedBookings);
        Assert.Equal(15, failedBookings);
    }

    [Fact]
    public async Task BookEvent_RaceCondition_UniqueIds()
    {
        //Arrange
        var totalSeats = 10;
        var eventId = await CreateEvent("event1", totalSeats: totalSeats);
        var uniqueBookingIds = new ConcurrentBag<Guid>();

        //Act
        var tasks = Enumerable.Range(0, totalSeats).Select(i => Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var booking = await bookingService.CreateBookingAsync(eventId);
            uniqueBookingIds.Add(booking.Id);
        }));

        await Task.WhenAll(tasks);

        //Assert
        Assert.Equal(totalSeats, uniqueBookingIds.Count);
    }

    private async Task<Guid> CreateEvent(string title, int totalSeats = 1)
    {
        var eventSource = new CreateEventRequestDto() { Title = title, StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 2), TotalSeats = totalSeats };
        var result = await _eventsService.AddEventAsync(eventSource);

        return result.Id;
    }
}
