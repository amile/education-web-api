using EducationWebApi.DAL;
using Microsoft.EntityFrameworkCore;

namespace EducationWebApi;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _dbContext;

    public BookingRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Booking> Add(Guid eventId)
    {
        var dbBooking = new BookingEntity()
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Status = BookingStatus.Pending.ToString(),
            CreatedAt = DateTime.UtcNow,
        };

        await _dbContext.Bookings.AddAsync(dbBooking);

        return Booking.FromDb(dbBooking);
    }

    public async Task<Booking?> GetById(Guid id)
    {
        var dbBooking = await _dbContext.Bookings.FirstOrDefaultAsync(b => b.Id == id);

        return dbBooking is not null ? Booking.FromDb(dbBooking) : null;
    }

    public async Task<bool> TryUpdate(Booking booking)
    {
        var dbBooking = await _dbContext.Bookings.FirstOrDefaultAsync(b => b.Id == booking.Id);

        if (dbBooking is not null)
        {
            _dbContext.Bookings.Update(dbBooking);

            return true;
        }

        return false;
    }

    public async Task<bool> TryUpdateStatus(Guid id, BookingStatus status)
    {
        var dbBooking = await _dbContext.Bookings.FirstOrDefaultAsync(b => b.Id == id);

        if (dbBooking is not null)
        {
            dbBooking.Status = status.ToString();
            dbBooking.ProcessedAt = DateTime.UtcNow;
            _dbContext.Bookings.Update(dbBooking);

            return true;
        }

        return false;
    }

    public async Task ConfirmBooking(Guid id)
    {
        await TryUpdateStatus(id, BookingStatus.Confirmed);
    }

    public async Task RejectBooking(Guid id)
    {
        await TryUpdateStatus(id, BookingStatus.Rejected);
    }

    public async Task SaveChanges()
    {
        await _dbContext.SaveChangesAsync();
    }
} 