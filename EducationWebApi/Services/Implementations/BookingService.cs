namespace EducationWebApi;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IEventsRepository _eventsRepository;

    public BookingService(
        IBookingRepository bookingRepository,
        IEventsRepository eventsRepository
    )
    {
        _bookingRepository = bookingRepository;
        _eventsRepository = eventsRepository;
    }

    public async Task<BookingDto> GetBookingByIdAsync(Guid bookingId)
    {
        var booking = await _bookingRepository.GetById(bookingId);

        if (booking is null)
        {
            throw new KeyNotFoundException($"Booking Id: {bookingId} not found");
        }

        return booking.ToApi();
    }

    public async Task<BookingDto> CreateBookingAsync(Guid eventId)
    {
        if (!_eventsRepository.TryGetEvent(eventId, out var _))
        {
            throw new KeyNotFoundException($"Event Id: {eventId} not found");
        }

        var booking = await _bookingRepository.Add(eventId);

        return booking.ToApi();
    }
}
