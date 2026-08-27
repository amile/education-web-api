namespace Bookings.Application;

public record CreateBookingRequestDto(
    Guid EventId,
    Guid UserId
);
