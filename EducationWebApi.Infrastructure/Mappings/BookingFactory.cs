using EducationWebApi.Domain;

namespace EducationWebApi.Infrastructure;

public class BookingFactory
{
    public static Booking FromDb(BookingEntity dbBooking) => new Booking()
    {
        Id = dbBooking.Id, 
        EventId = dbBooking.EventId,
        UserId = dbBooking.UserId,
        Status = Enum.Parse<BookingStatus>(dbBooking.Status),
        CreatedAt = dbBooking.CreatedAt, 
        ProcessedAt = dbBooking.ProcessedAt
    };

    public static BookingEntity ToDb(Booking booking) => new BookingEntity()
    {
        Id = booking.Id, 
        EventId = booking.EventId,
        UserId = booking.UserId, 
        Status = booking.Status.ToString(),
        CreatedAt = booking.CreatedAt, 
        ProcessedAt = booking.ProcessedAt
    };
}