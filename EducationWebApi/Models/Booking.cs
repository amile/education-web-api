namespace EducationWebApi;

public class Booking
{
    public required Guid Id { get; set; }
    public required Guid EventId { get; set; }
    public required BookingStatus Status { get; set; }
    public required DateTime StartAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public BookingDto ToApi() => new BookingDto(Id, EventId, Status);
}