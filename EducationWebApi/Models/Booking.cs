using EducationWebApi.DAL;
using Npgsql.Replication;

namespace EducationWebApi;

public class Booking
{
    public required Guid Id { get; set; }
    public required Guid EventId { get; set; }
    public required BookingStatus Status { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public void ConfirmBooking()
    {
        Status = BookingStatus.Confirmed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void RejectBooking()
    {
        Status = BookingStatus.Rejected;
        ProcessedAt = DateTime.UtcNow;
    }

    public BookingDto ToApi() => new BookingDto(Id, EventId, Status, CreatedAt, ProcessedAt);

    public static Booking FromDb(BookingEntity dbBooking) => new Booking()
    {
        Id = dbBooking.Id, 
        EventId = dbBooking.EventId, 
        Status = Enum.Parse<BookingStatus>(dbBooking.Status),
        CreatedAt = dbBooking.CreatedAt, 
        ProcessedAt = dbBooking.ProcessedAt
    };

    public BookingEntity ToDb() => new BookingEntity()
    {
        Id = Id, 
        EventId = EventId, 
        Status = Status.ToString(),
        CreatedAt = CreatedAt, 
        ProcessedAt = ProcessedAt
    };
}