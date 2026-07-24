using EducationWebApi.Domain;

namespace EducationWebApi.Application;

public record BookingDto(
    Guid Id,
    Guid EventId,
    BookingStatus Status,
    DateTime CreatedAt,
    DateTime? ProcessedAt
)
{
    public static BookingDto FromDomain(Booking model) => new BookingDto(
        model.Id, model.EventId, model.Status, model.CreatedAt, model.ProcessedAt);
}
