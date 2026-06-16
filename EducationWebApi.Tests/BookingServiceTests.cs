namespace EducationWebApi.Tests;

public class BookingServiceTests
{
    private readonly IEventsRepository _eventsRepository;
    private readonly IEventsService _eventsService;
    private readonly IBookingRepository _bookingRepository;
    private readonly IBookingService _bookingService;

    public BookingServiceTests()
    {
        _eventsRepository = new EventsRepository();
        _eventsService = new EventsService(_eventsRepository);
        _bookingRepository = new BookingRepository();
        _bookingService = new BookingService(_bookingRepository, _eventsRepository);
    }

    [Fact]
    public async Task BookEvent_Ok()
    {
        //Arrange
        var eventId = CreateEvent("event1");

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
        var eventId = CreateEvent("event1", totalSeats: 3);

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
        var eventId = CreateEvent("event1", totalSeats: 3);

        //Act
        await _bookingService.CreateBookingAsync(eventId);
        var actualEvent = _eventsService.GetEvent(eventId);

        //Assert
        Assert.NotNull(actualEvent);
        Assert.Equal(eventId, actualEvent.Id);
        Assert.Equal(2, actualEvent.AvailableSeats);
    }

    [Fact]
    public async Task BookEventMultiple_ReserveSeats_Ok()
    {
        //Arrange
        var eventId = CreateEvent("event1", totalSeats: 3);

        //Act
        var actualBooking1 = await _bookingService.CreateBookingAsync(eventId);
        var actualBooking2 = await _bookingService.CreateBookingAsync(eventId);
        var actualBooking3 = await _bookingService.CreateBookingAsync(eventId);

        var actualEvent = _eventsService.GetEvent(eventId);

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
        var eventId = CreateEvent("event1");
        await _bookingService.CreateBookingAsync(eventId);

        //Assert
        var error = await Assert.ThrowsAsync<NoAvailableSeatsException>(() => _bookingService.CreateBookingAsync(eventId));
    }

    [Fact]
    public async Task GetBooking_Ok()
    {
        //Arrange
        var eventId = CreateEvent("event1");
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
    public async Task ConfirmBooking_Ok()
    {
        //Arrange
        var eventId = CreateEvent("event1");
        var pendingBooking = await _bookingService.CreateBookingAsync(eventId);

        //Act
        _bookingRepository.ConfirmBooking(pendingBooking.Id);
        var confirmedBooking = await _bookingService.GetBookingByIdAsync(pendingBooking.Id);

        //Assert
        Assert.Equal(pendingBooking.Id, confirmedBooking.Id);
        Assert.Equal(BookingStatus.Pending, pendingBooking.Status);
        Assert.Equal(BookingStatus.Confirmed, confirmedBooking.Status);
        Assert.NotNull(confirmedBooking.ProcessedAt);
    }

    [Fact]
    public async Task RejectBooking_Ok()
    {
        //Arrange
        var eventId = CreateEvent("event1");
        var pendingBooking = await _bookingService.CreateBookingAsync(eventId);

        //Act
        _bookingRepository.RejectBooking(pendingBooking.Id);
        var rejectedBooking = await _bookingService.GetBookingByIdAsync(pendingBooking.Id);

        //Assert
        Assert.Equal(pendingBooking.Id, rejectedBooking.Id);
        Assert.Equal(BookingStatus.Pending, pendingBooking.Status);
        Assert.Equal(BookingStatus.Rejected, rejectedBooking.Status);
        Assert.NotNull(rejectedBooking.ProcessedAt);
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
        var eventId = CreateEvent("event1");
        _eventsService.RemoveEvent(eventId);

        //Assert
        var error = await Assert.ThrowsAsync<KeyNotFoundException>(() => _bookingService.CreateBookingAsync(eventId));
        Assert.Equal($"Event Id: {eventId} not found", error.Message);
    }

    [Fact]
    public async Task GetBooking_WrongId()
    {
        //Arrange
        var eventId = CreateEvent("event1");
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
        var eventId = CreateEvent("event1", totalSeats: 5);
        var tasks = new Task[20];

        //Act
        foreach (var idx in Enumerable.Range(0, 20))
        {
            tasks[idx] = _bookingService.CreateBookingAsync(eventId);
        }

        var allTasks = Task.WhenAll(tasks);

        try
        {
            await allTasks;
        }
        catch
        {
        }

        var completedBookings = 0;
        var failedBookings = 0;
        foreach (var task in tasks)
        {
            if (task.Status == TaskStatus.RanToCompletion)
            {
                completedBookings++;
            }
            else if (task.Status == TaskStatus.Faulted)
            {
                failedBookings++;
            }
        }

        var actualEvent = _eventsService.GetEvent(eventId);

        //Assert
        Assert.Equal(0, actualEvent.AvailableSeats);
        Assert.Equal(5, completedBookings);
        Assert.Equal(15, failedBookings);
        foreach (var ex in allTasks.Exception!.InnerExceptions)
        {
            Assert.Equal("No available seats for this event", ex.Message);
        }
    }

    [Fact]
    public async Task BookEvent_RaceCondition_UniqueIds()
    {
        //Arrange
        var eventId = CreateEvent("event1", totalSeats: 10);
        var tasks = new Task<BookingDto>[10];

        //Act
        foreach (var idx in Enumerable.Range(0, 10))
        {
            tasks[idx] = _bookingService.CreateBookingAsync(eventId);
        }

        try
        {
            await Task.WhenAll(tasks);
        }
        catch
        {
        }

        var uniqueBookingIds = new HashSet<Guid>();
        foreach (var task in tasks)
        {
            if (task.Status == TaskStatus.RanToCompletion)
            {
                var result = await task;
                uniqueBookingIds.Add(result.Id);
            }
        }

        var actualEvent = _eventsService.GetEvent(eventId);

        //Assert
        Assert.Equal(0, actualEvent.AvailableSeats);
        Assert.Equal(10, uniqueBookingIds.Count);
    }

    private Guid CreateEvent(string title, int totalSeats = 1)
    {
        var eventSource = new CreateEventRequestDto() { Title = title, StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 2), TotalSeats = totalSeats };
        var result = _eventsService.AddEvent(eventSource);

        return result.Id;
    }
}
