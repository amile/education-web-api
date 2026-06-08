using System.Linq;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace EducationWebApi;

public class BookingRepository : IBookingRepository
{
    private readonly ConcurrentDictionary<Guid, Booking> _bookings;

    public BookingRepository()
    {
        _bookings = new ConcurrentDictionary<Guid, Booking>();
    }

    public async Task<Booking> Add(Guid eventId)
    {
        var booking = new Booking()
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };

        _bookings.TryAdd(booking.Id, booking);

        return booking;
    }

    public async Task<Booking?> GetById(Guid id)
    {
        return _bookings.GetValueOrDefault(id);
    }

    public async Task<List<Booking>> GetAllPendingBookings()
    {
        return _bookings.Where(item => item.Value.Status == BookingStatus.Pending).Select(item => item.Value).ToList();
    }

    public async Task<bool> TryUpdate(Booking booking)
    {
        if (_bookings.ContainsKey(booking.Id))
        {
            _bookings[booking.Id] = booking;
            return true;
        }

        return false;
    }

    public async Task<bool> TryUpdateStatus(Guid id, BookingStatus status)
    {
        if (_bookings.TryGetValue(id, out var booking))
        {
            booking.Status = status;
            booking.ProcessedAt = DateTime.UtcNow;
            _bookings[booking.Id] = booking;

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
} 