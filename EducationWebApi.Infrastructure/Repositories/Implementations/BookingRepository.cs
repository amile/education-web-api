using EducationWebApi.Application;
using EducationWebApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace EducationWebApi.Infrastructure;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _dbContext;

    public BookingRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Booking> AddBookingAsync(Guid eventId, CancellationToken ct = default)
    {
        var dbBooking = new BookingEntity()
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Status = BookingStatus.Pending.ToString(),
            CreatedAt = DateTime.UtcNow,
        };

        await _dbContext.Bookings.AddAsync(dbBooking);

        return BookingFactory.FromDb(dbBooking);
    }

    public async Task<Booking?> GetBookingByIdAsync(Guid id, CancellationToken ct = default)
    {
        var dbBooking = await _dbContext.Bookings.FirstOrDefaultAsync(b => b.Id == id);

        return dbBooking is not null ? BookingFactory.FromDb(dbBooking) : null;
    }

    public Task<List<Booking>> GetPendingBookingsAsync(CancellationToken ct = default)
    {
        return _dbContext.Bookings
            .Where(item => item.Status == BookingStatus.Pending.ToString())
            .Select(item => BookingFactory.FromDb(item))
            .ToListAsync(ct);
    }

    public async Task UpdateStatusAsync(Guid id, BookingStatus status, CancellationToken ct = default)
    {
        var dbBooking = await _dbContext.Bookings.FirstOrDefaultAsync(b => b.Id == id);

        if (dbBooking is null)
        {
            throw new KeyNotFoundException($"Booking Id: {id} not found");
        }

        dbBooking.Status = status.ToString();
        dbBooking.ProcessedAt = DateTime.UtcNow;
        _dbContext.Bookings.Update(dbBooking);
    }

    public Task ConfirmBookingAsync(Guid id, CancellationToken ct = default)
    {
        return UpdateStatusAsync(id, BookingStatus.Confirmed, ct);
    }

    public Task RejectBookingAsync(Guid id, CancellationToken ct = default)
    {
        return UpdateStatusAsync(id, BookingStatus.Rejected, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _dbContext.SaveChangesAsync();
    }
} 