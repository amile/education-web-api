namespace EducationWebApi;

public record BookingDto(
    Guid Id,
    Guid EventId,
    BookingStatus Status
);
