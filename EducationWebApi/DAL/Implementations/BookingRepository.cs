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

    public Booking Add(Guid eventId)
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

    public Booking? GetById(Guid id)
    {
        return _bookings.GetValueOrDefault(id);
    }

    public List<Booking> GetAllPendingBookings()
    {
        return _bookings.Where(item => item.Value.Status == BookingStatus.Pending).Select(item => item.Value).ToList();
    }

    public bool TryUpdate(Booking booking)
    {
        if (_bookings.ContainsKey(booking.Id))
        {
            _bookings[booking.Id] = booking;
            return true;
        }

        return false;
    }

    public bool TryUpdateStatus(Guid id, BookingStatus status)
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

    public void ConfirmBooking(Guid id)
    {
        TryUpdateStatus(id, BookingStatus.Confirmed);
    }

    public void RejectBooking(Guid id)
    {
        TryUpdateStatus(id, BookingStatus.Rejected);
    }
} 