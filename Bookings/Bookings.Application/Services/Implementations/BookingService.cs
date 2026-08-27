using Bookings.Domain;
using Contracts;

namespace Bookings.Application;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;

    private readonly IBookingProviderService _bookingProvider;
    private static readonly SemaphoreSlim _processingSemaphore = new(1, 1);
    private const int MaxUserBookingsCount = 10;

    public BookingService(
        IBookingRepository bookingRepository,
        IBookingProviderService bookingProvider
    )
    {
        _bookingRepository = bookingRepository;
        _bookingProvider = bookingProvider;
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
            var userActiveBookings = await _bookingRepository.GetActiveBookingsByUserAsync(userId, cancellationToken);

            if (userActiveBookings.Count >= MaxUserBookingsCount)
            {
                throw new TooManyBookingsException();
            }

            var booking = await _bookingRepository.AddBookingAsync(eventId, userId, cancellationToken);
            await _bookingRepository.SaveChangesAsync(cancellationToken);

            await _bookingProvider.Publish(booking, cancellationToken);

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
