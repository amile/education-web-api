using EducationWebApi.Domain;

namespace EducationWebApi.Application;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IEventsRepository _eventsRepository;
    private static readonly SemaphoreSlim _processingSemaphore = new(1, 1);

    public BookingService(
        IBookingRepository bookingRepository,
        IEventsRepository eventsRepository
    )
    {
        _bookingRepository = bookingRepository;
        _eventsRepository = eventsRepository;
    }

    public async Task<BookingDto> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId, cancellationToken);

        if (booking is null)
        {
            throw new KeyNotFoundException($"Booking Id: {bookingId} not found");
        }

        return BookingDto.FromDomain(booking);
    }

    public async Task<BookingDto> CreateBookingAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default)
    {
        await _processingSemaphore.WaitAsync();

        try
        {
            var domainEvent = await _eventsRepository.GetEventByIdAsync(eventId, cancellationToken);

            if (domainEvent is null)
            {
                throw new KeyNotFoundException($"Event Id: {eventId} not found");
            }

            if (!domainEvent.TryReserveSeats())
            {
                throw new NoAvailableSeatsException();
            }

            await _eventsRepository.ChangeEventAsync(domainEvent, cancellationToken);

            var booking = await _bookingRepository.AddBookingAsync(eventId, userId, cancellationToken);
            await _bookingRepository.SaveChangesAsync(cancellationToken);

            return BookingDto.FromDomain(booking);
        }
        finally
        {
            _processingSemaphore.Release();
        }
    }
}
