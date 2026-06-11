using System.Diagnostics.CodeAnalysis;

namespace EducationWebApi;

public interface IBookingRepository
{
    Booking Add(Guid eventId);
    Booking? GetById(Guid id);
    List<Booking> GetAllPendingBookings();
    bool TryUpdate(Booking booking);
    bool TryUpdateStatus(Guid id, BookingStatus status);
    void ConfirmBooking(Guid id);
    void RejectBooking(Guid id);
} 