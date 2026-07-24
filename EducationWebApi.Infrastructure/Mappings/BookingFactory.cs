using EducationWebApi.Domain;

namespace EducationWebApi.Infrastructure;

public class BookingFactory
{
    public static Booking FromDb(BookingEntity dbBooking) => new Booking()
    {
        Id = dbBooking.Id, 
        EventId = dbBooking.EventId, 
        Status = Enum.Parse<BookingStatus>(dbBooking.Status),
        CreatedAt = dbBooking.CreatedAt, 
        ProcessedAt = dbBooking.ProcessedAt
    };

    public static BookingEntity ToDb(Booking booking) => new BookingEntity()
    {
        Id = booking.Id, 
        EventId = booking.EventId, 
        Status = booking.Status.ToString(),
        CreatedAt = booking.CreatedAt, 
        ProcessedAt = booking.ProcessedAt
    };
}