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
            StartAt = DateTime.UtcNow,
        };

        _bookings.TryAdd(booking.Id, booking);

        return booking;
    }

    public async Task<Booking?> GetById(Guid id)
    {
        return _bookings.GetValueOrDefault(id);
    }

    public async Task<Booking?> GetPending()
    {
        return _bookings.FirstOrDefault(item => item.Value.Status == BookingStatus.Pending).Value;
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
} 