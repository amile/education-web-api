using EducationWebApi.Domain;

namespace EducationWebApi.Application;

public interface IBookingRepository
{
    Task<Booking> AddBookingAsync(Guid eventId, Guid userId, CancellationToken ct = default);
    Task<Booking?> GetBookingByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Booking>> GetPendingBookingsAsync(CancellationToken ct = default);
    Task UpdateStatusAsync(Guid id, BookingStatus status, CancellationToken ct = default);
    Task ConfirmBookingAsync(Guid id, CancellationToken ct = default);
    Task RejectBookingAsync(Guid id, CancellationToken ct = default);
    Task CancelBookingAsync(Guid id, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
} 