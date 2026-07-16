
namespace EducationWebApi.Infrastructure;

public class EventEntity
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required DateTime StartAt { get; set; }
    public required DateTime EndAt { get; set; }
    public required int TotalSeats { get; set; }
    public required int AvailableSeats { get; set; }
    public List<BookingEntity>? Bookings { get; set; }
}