namespace Contracts;

public record BookingConfirmed(
    Guid BookingId,
    Guid EventId,
    Guid UserId,
    int SeatsAmount,
    DateTime ProcessedAt
);
