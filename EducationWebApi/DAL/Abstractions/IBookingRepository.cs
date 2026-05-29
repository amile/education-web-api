namespace EducationWebApi;

public interface IBookingRepository
{
    Task<Guid> Add(Guid eventId);
    Task<Booking?> GetById(Guid id);
} 