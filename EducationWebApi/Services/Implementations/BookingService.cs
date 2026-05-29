namespace EducationWebApi;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;

    public BookingService(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<Booking> GetBookingByIdAsync(Guid bookingId)
    {
        var booking = await _bookingRepository.GetById(bookingId);

        return booking ?? throw new KeyNotFoundException($"Booking Id: {bookingId} not found");
    }

    public async Task<Guid> CreateBookingAsync(Guid eventId)
    {
        return await _bookingRepository.Add(eventId);
    }
}
