namespace EducationWebApi;

public interface IBookingService
{
    Task<BookingDto> CreateBookingAsync(Guid eventId);
    Task<BookingDto?> GetBookingByIdAsync(Guid bookingId);
}
