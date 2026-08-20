namespace EducationWebApi.Domain;

public class Booking
{
    public required Guid Id { get; set; }
    public required Guid EventId { get; set; }
    public required Guid UserId { get; set; }
    public required BookingStatus Status { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public void ConfirmBooking()
    {
        Status = BookingStatus.Confirmed;
        ProcessedAt = DateTime.UtcNow;
    }

    public bool CancelBooking()
    {
        if (Status == BookingStatus.Cancelled)
        {
            return false;
        }

        Status = BookingStatus.Cancelled;
        ProcessedAt = DateTime.UtcNow;
        return true;
    }

    public void RejectBooking()
    {
        Status = BookingStatus.Rejected;
        ProcessedAt = DateTime.UtcNow;
    }
}