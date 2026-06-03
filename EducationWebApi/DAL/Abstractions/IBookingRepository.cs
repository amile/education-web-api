using System.Diagnostics.CodeAnalysis;

namespace EducationWebApi;

public interface IBookingRepository
{
    Task<Booking> Add(Guid eventId);
    Task<Booking?> GetById(Guid id);
    Task<List<Booking>> GetAllPendingBookings();
    Task<bool> TryUpdate(Booking booking);
    Task<bool> TryUpdateStatus(Guid id, BookingStatus status);
    Task ConfirmBooking(Guid id);
    Task RejectBooking(Guid id);
} 