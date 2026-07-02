using EducationWebApi.DAL;
using Microsoft.EntityFrameworkCore;

namespace EducationWebApi;

public class BookingService : IBookingService
{
    private readonly AppDbContext _dbContext;
    private static readonly SemaphoreSlim _processingSemaphore = new(1, 1);

    public BookingService(
        AppDbContext dbContext
    )
    {
        _dbContext = dbContext;
    }

    public async Task<BookingDto> GetBookingByIdAsync(Guid bookingId)
    {
        var dbBooking = await _dbContext.Bookings.FirstOrDefaultAsync(e => e.Id == bookingId);

        if (dbBooking is null)
        {
            throw new KeyNotFoundException($"Booking Id: {bookingId} not found");
        }

        return Booking.FromDb(dbBooking).ToApi();
    }

    public async Task<BookingDto> CreateBookingAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        await _processingSemaphore.WaitAsync();

        try
        {
            var dbEvent = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);

            if (dbEvent is null)
            {
                throw new KeyNotFoundException($"Event Id: {eventId} not found");
            }

            var domainEvent = Event.FromDb(dbEvent);

            if (!domainEvent.TryReserveSeats())
            {
                throw new NoAvailableSeatsException();
            }

            dbEvent.AvailableSeats = domainEvent.AvailableSeats;
            _dbContext.Events.Update(dbEvent);

            var booking = new Booking()
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow,
            };
            await _dbContext.Bookings.AddAsync(booking.ToDb(), cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return booking.ToApi();
        }
        finally
        {
            _processingSemaphore.Release();
        }
    }
}
