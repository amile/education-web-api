using System.Collections.Concurrent;
using Bookings.Application;
using Bookings.Domain;
using Bookings.Infrastructure;
using Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bookings.Tests;

public class BookingServiceTests
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IServiceScope _scope;
    private readonly IBookingService _bookingService;

    public BookingServiceTests()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        
        services.AddLogging();

        services.Configure<KafkaConfig>(options =>
        {
            options.BootstrapServers = "localhost:9092";
        });
        services.AddSingleton<IBookingProviderService, BookingProviderService>();

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IBookingService, BookingService>();

        _serviceProvider = services.BuildServiceProvider();
        _scope = _serviceProvider.CreateScope();
        _bookingService = _scope.ServiceProvider.GetRequiredService<IBookingService>();
    }

    [Fact]
    public async Task BookEvent_Ok()
    {
        //Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        //Act
        var actual = await _bookingService.CreateBookingAsync(eventId, userId);

        //Assert
        Assert.NotNull(actual);
        Assert.Equal(eventId, actual.EventId);
        Assert.Equal(BookingStatus.Pending, actual.Status);
    }

    [Fact]
    public async Task BookEventMultiple_Ok()
    {
        //Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        //Act
        var actualBooking1 = await _bookingService.CreateBookingAsync(eventId, userId);
        var actualBooking2 = await _bookingService.CreateBookingAsync(eventId, userId);
        var actualBooking3 = await _bookingService.CreateBookingAsync(eventId, userId);

        //Assert
        Assert.NotNull(actualBooking1);
        Assert.NotNull(actualBooking2);
        Assert.NotNull(actualBooking3);
        Assert.NotEqual(actualBooking1.Id, actualBooking2.Id);
        Assert.NotEqual(actualBooking1.Id, actualBooking3.Id);
        Assert.NotEqual(actualBooking2.Id, actualBooking3.Id);
    }

    [Fact]
    public async Task GetBookingByOwner_Ok()
    {
        //Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var expectedBooking = await _bookingService.CreateBookingAsync(eventId, userId);

        //Act
        var actualBooking = await _bookingService.GetBookingByIdAsync(expectedBooking.Id, userId, null);

        //Assert
        Assert.NotNull(actualBooking);
        Assert.Equal(expectedBooking.Id, actualBooking.Id);
        Assert.Equal(expectedBooking.EventId, actualBooking.EventId);
        Assert.Equal(expectedBooking.Status, actualBooking.Status);
    }

    [Fact]
    public async Task GetBookingByAdmin_Ok()
    {
        //Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var expectedBooking = await _bookingService.CreateBookingAsync(eventId, userId);

        //Act
        var actualBooking = await _bookingService.GetBookingByIdAsync(expectedBooking.Id, Guid.NewGuid(), UserRole.Admin.ToString());

        //Assert
        Assert.NotNull(actualBooking);
        Assert.Equal(expectedBooking.Id, actualBooking.Id);
        Assert.Equal(expectedBooking.EventId, actualBooking.EventId);
        Assert.Equal(expectedBooking.Status, actualBooking.Status);
    }

    [Fact]
    public async Task BookEvent_UserLimit()
    {
        //Arrange
        var totalSeats = 11;
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        //Act
        var tasks = Enumerable.Range(0, totalSeats).Select(i => Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

            try
            {
                await bookingService.CreateBookingAsync(eventId, userId);
                return true;
            }
            catch (TooManyBookingsException)
            {
                return false;
            }
        }));

        var results = await Task.WhenAll(tasks);
        var completedBookings = results.Count(r => r);
        var failedBookings = totalSeats - completedBookings;

        //Assert
        Assert.Equal(10, completedBookings);
        Assert.Equal(1, failedBookings);
    }

    [Fact]
    public async Task BookEvent_DifferentUsersLimit()
    {
        //Arrange
        var totalSeats = 11;
        var eventId = Guid.NewGuid();
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        var user1BookingsIds = new ConcurrentBag<Guid>();

        //Act
        var tasks = Enumerable.Range(0, 10).Select(i => Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var booking = await bookingService.CreateBookingAsync(eventId, user1Id);
            user1BookingsIds.Add(booking.Id);
        }));

        await Task.WhenAll(tasks);

        var user2Booking = await _bookingService.CreateBookingAsync(eventId, user2Id);

        //Assert
        Assert.Equal(10, user1BookingsIds.Count);
        Assert.NotNull(user2Booking);
    }

    [Fact]
    public async Task GetBooking_WrongId()
    {
        //Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await _bookingService.CreateBookingAsync(eventId, userId);
        var wrongId = Guid.NewGuid();

        //Assert
        var error = await Assert.ThrowsAsync<NotFoundException>(() => _bookingService.GetBookingByIdAsync(wrongId, userId, null));
        Assert.Equal($"Booking Id: {wrongId} not found", error.Message);
    }

    [Fact]
    public async Task GetBooking_Forbidden()
    {
        //Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var booking = await _bookingService.CreateBookingAsync(eventId, userId);

        //Assert
        var error = await Assert.ThrowsAsync<NoPermissionException>(() => _bookingService.GetBookingByIdAsync(booking.Id, Guid.NewGuid(), null));
        Assert.Equal($"No permission to perform the requested action", error.Message);
    }

    [Fact]
    public async Task BookEvent_RaceCondition_UniqueIds()
    {
        //Arrange
        var totalSeats = 10;
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var uniqueBookingIds = new ConcurrentBag<Guid>();

        //Act
        var tasks = Enumerable.Range(0, totalSeats).Select(i => Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var booking = await bookingService.CreateBookingAsync(eventId, userId);
            uniqueBookingIds.Add(booking.Id);
        }));

        await Task.WhenAll(tasks);

        //Assert
        Assert.Equal(totalSeats, uniqueBookingIds.Count);
    }

    [Fact]
    public async Task CancelBookingByOwner_Ok()
    {
        //Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var booking = await _bookingService.CreateBookingAsync(eventId, userId);

        //Act
        var cancelledBooking = await _bookingService.CancelBookingAsync(booking.Id, userId, null);

        //Assert
        Assert.NotNull(cancelledBooking);
        Assert.Equal(BookingStatus.Cancelled, cancelledBooking.Status);
    }

    [Fact]
    public async Task CancelBookingByAdmin_Ok()
    {
        //Arrange
        var eventId = Guid.NewGuid();
        var booking = await _bookingService.CreateBookingAsync(eventId, Guid.NewGuid());

        //Act
        var cancelledBooking = await _bookingService.CancelBookingAsync(booking.Id, Guid.NewGuid(), UserRole.Admin.ToString());

        //Assert
        Assert.NotNull(cancelledBooking);
        Assert.Equal(BookingStatus.Cancelled, cancelledBooking.Status);
    }

    [Fact]
    public async Task CancelBooking_Forbidden()
    {
        //Arrange
        var eventId = Guid.NewGuid();
        var booking = await _bookingService.CreateBookingAsync(eventId, Guid.NewGuid());

        //Assert
        var error = await Assert.ThrowsAsync<NoPermissionException>(() => _bookingService.CancelBookingAsync(booking.Id, Guid.NewGuid(), UserRole.User.ToString()));
        Assert.Equal("No permission to perform the requested action", error.Message);
    }
}
