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
        var eventItem = new EventRequestDto() { Title = "event1", StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 2), TotalSeats = 1 };
        var eventId = _eventsService.AddEvent(eventItem).Id;

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
        var eventItem = new EventRequestDto() { Title = "event1", StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 2), TotalSeats = 1 };
        var eventId = _eventsService.AddEvent(eventItem).Id;

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
    public async Task GetBooking_Ok()
    {
        //Arrange
        var eventItem = new EventRequestDto() { Title = "event1", StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 2), TotalSeats = 1 };
        var eventId = _eventsService.AddEvent(eventItem).Id;
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
        var eventItem = new EventRequestDto() { Title = "event1", StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 2), TotalSeats = 1 };
        var eventId = _eventsService.AddEvent(eventItem).Id;
        var pendingBooking = await _bookingService.CreateBookingAsync(eventId);

        //Act
        await _bookingRepository.ConfirmBooking(pendingBooking.Id);
        var confirmedBooking = await _bookingService.GetBookingByIdAsync(pendingBooking.Id);

        //Assert
        Assert.Equal(pendingBooking.Id, confirmedBooking.Id);
        Assert.Equal(BookingStatus.Pending, pendingBooking.Status);
        Assert.Equal(BookingStatus.Confirmed, confirmedBooking.Status);
    }

    [Fact]
    public async Task RejectBooking_Ok()
    {
        //Arrange
        var eventItem = new EventRequestDto() { Title = "event1", StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 2), TotalSeats = 1 };
        var eventId = _eventsService.AddEvent(eventItem).Id;
        var pendingBooking = await _bookingService.CreateBookingAsync(eventId);

        //Act
        await _bookingRepository.RejectBooking(pendingBooking.Id);
        var rejectedBooking = await _bookingService.GetBookingByIdAsync(pendingBooking.Id);

        //Assert
        Assert.Equal(pendingBooking.Id, rejectedBooking.Id);
        Assert.Equal(BookingStatus.Pending, pendingBooking.Status);
        Assert.Equal(BookingStatus.Rejected, rejectedBooking.Status);
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
        var eventItem = new EventRequestDto() { Title = "event1", StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 2), TotalSeats = 1 };
        var eventId = _eventsService.AddEvent(eventItem).Id;
        _eventsService.RemoveEvent(eventId);

        //Assert
        var error = await Assert.ThrowsAsync<KeyNotFoundException>(() => _bookingService.CreateBookingAsync(eventId));
        Assert.Equal($"Event Id: {eventId} not found", error.Message);
    }

    [Fact]
    public async Task GetBooking_WrongId()
    {
        //Arrange
        var eventItem = new EventRequestDto() { Title = "event1", StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 2), TotalSeats = 1 };
        var eventId = _eventsService.AddEvent(eventItem).Id;
        await _bookingService.CreateBookingAsync(eventId);
        var wrongId = Guid.NewGuid();

        //Assert
        var error = await Assert.ThrowsAsync<KeyNotFoundException>(() => _bookingService.GetBookingByIdAsync(wrongId));
        Assert.Equal($"Booking Id: {wrongId} not found", error.Message);
    }
}
