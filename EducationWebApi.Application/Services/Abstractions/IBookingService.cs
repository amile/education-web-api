namespace EducationWebApi.Application;

public interface IBookingService
{
    Task<BookingDto> CreateBookingAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default);
    Task<BookingDto> GetBookingByIdAsync(Guid bookingId, Guid userId, string? userRole, CancellationToken cancellationToken = default);
    Task<BookingDto> CancelBookingAsync(Guid bookingId, Guid userId, string? userRole, CancellationToken cancellationToken = default);
}
