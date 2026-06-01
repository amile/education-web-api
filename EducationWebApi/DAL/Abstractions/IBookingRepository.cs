using System.Diagnostics.CodeAnalysis;

namespace EducationWebApi;

public interface IBookingRepository
{
    Task<Booking> Add(Guid eventId);
    Task<Booking?> GetById(Guid id);
    Task<Booking?> GetPending();
    Task<bool> TryUpdate(Booking booking);
} 