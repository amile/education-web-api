namespace EducationWebApi;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IEventsRepository _eventsRepository;
    private readonly object _bookingLock = new();

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
        var booking = _bookingRepository.GetById(bookingId);

        if (booking is null)
        {
            throw new KeyNotFoundException($"Booking Id: {bookingId} not found");
        }

        return booking.ToApi();
    }

    public async Task<BookingDto> CreateBookingAsync(Guid eventId)
    {
        Booking? booking = null;

        lock (_bookingLock)
        {
            if (!_eventsRepository.TryGetEvent(eventId, out var eventItem))
            {
                throw new KeyNotFoundException($"Event Id: {eventId} not found");
            }

            if (!eventItem.TryReserveSeats())
            {
                throw new NoAvailableSeatsException();
            }
            _eventsRepository.TryChangeEvent(eventItem);

            booking = _bookingRepository.Add(eventId);
        }

        return booking.ToApi();
    }
}
