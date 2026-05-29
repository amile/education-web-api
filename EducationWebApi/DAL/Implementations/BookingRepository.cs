using System.Collections.Concurrent;

namespace EducationWebApi;

public class BookingRepository : IBookingRepository
{
    private readonly ConcurrentDictionary<Guid, Booking> _bookings;

    public BookingRepository()
    {
        _bookings = new ConcurrentDictionary<Guid, Booking>();
    }

    public async Task<Guid> Add(Guid eventId)
    {
        var booking = new Booking()
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Status = BookingStatus.Pending,
            StartAt = DateTime.UtcNow,
        };

        _bookings.TryAdd(booking.Id, booking);

        return booking.Id;
    }

    public async Task<Booking?> GetById(Guid id)
    {
        return _bookings.GetValueOrDefault(id);
    }
} 