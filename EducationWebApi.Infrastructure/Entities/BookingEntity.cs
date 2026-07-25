namespace EducationWebApi.Infrastructure;

public class BookingEntity
{
    public required Guid Id { get; set; }
    public required Guid EventId { get; set; }
    public required Guid UserId { get; set; }
    public required string Status { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public EventEntity? Event { get; set; }
    public UserEntity? User { get; set; }
}