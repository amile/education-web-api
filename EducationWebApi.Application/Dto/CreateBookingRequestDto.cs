namespace EducationWebApi.Application;

public record CreateBookingRequestDto(
    Guid EventId,
    Guid UserId
);
