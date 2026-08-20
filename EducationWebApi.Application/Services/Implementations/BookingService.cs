using EducationWebApi.Domain;

namespace EducationWebApi.Application;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IEventsRepository _eventsRepository;
    private static readonly SemaphoreSlim _processingSemaphore = new(1, 1);
    private const int MaxUserBookingsCount = 10;

    public BookingService(
        IBookingRepository bookingRepository,
        IEventsRepository eventsRepository
    )
    {
        _bookingRepository = bookingRepository;
        _eventsRepository = eventsRepository;
    }

    public async Task<BookingDto> GetBookingByIdAsync(Guid bookingId, Guid userId, string? userRole, CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId, cancellationToken);

        if (booking is null)
        {
            throw new NotFoundException($"Booking Id: {bookingId} not found");
        }

        var isOwner = booking.UserId == userId;
        var isAdmin = string.Equals(userRole, UserRole.Admin.ToString(), StringComparison.Ordinal);
        if (!isOwner && !isAdmin)
        {
            throw new NoPermissionException();
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
                throw new NotFoundException($"Event Id: {eventId} not found");
            }

            if (domainEvent.AlreadyStarted())
            {
                throw new EventAlreadyStartedException();
            }

            if (!domainEvent.TryReserveSeats())
            {
                throw new NoAvailableSeatsException();
            }

            var userActiveBookings = await _bookingRepository.GetActiveBookingsByUserAsync(userId, cancellationToken);

            if (userActiveBookings.Count >= MaxUserBookingsCount)
            {
                throw new TooManyBookingsException();
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

    public async Task<BookingDto> CancelBookingAsync(Guid bookingId, Guid userId, string? userRole, CancellationToken cancellationToken = default)
    {
        await _processingSemaphore.WaitAsync();

        try
        {
            var booking = await _bookingRepository.GetBookingByIdAsync(bookingId, cancellationToken);

            if (booking is null)
            {
                throw new NotFoundException($"Booking Id: {bookingId} not found");
            }

            var isOwner = booking.UserId == userId;
            var isAdmin = string.Equals(userRole, UserRole.Admin.ToString(), StringComparison.Ordinal);
            if (!isOwner && !isAdmin)
            {
                throw new NoPermissionException();
            }

            var domainEvent = await _eventsRepository.GetEventByIdAsync(booking.EventId, cancellationToken) ?? throw new NotFoundException($"Event Id: {booking.EventId} not found");

            if (domainEvent.AlreadyStarted())
            {
                throw new EventAlreadyStartedException();
            }

            if (!booking.CancelBooking())
            {
                throw new BookingAlreadyCancelledException();
            }

            await _bookingRepository.CancelBookingAsync(booking.Id, cancellationToken);
            await _bookingRepository.SaveChangesAsync(cancellationToken);

            return BookingDto.FromDomain(booking);
        }
        finally
        {
            _processingSemaphore.Release();
        }
    }
}
