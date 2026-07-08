namespace EducationWebApi;

public interface IBookingService
{
    Task<BookingDto> CreateBookingAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<BookingDto> GetBookingByIdAsync(Guid bookingId);
}
