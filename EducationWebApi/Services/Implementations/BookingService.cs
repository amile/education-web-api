namespace EducationWebApi;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;

    public BookingService(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<BookingDto?> GetBookingByIdAsync(Guid bookingId)
    {
        var booking = await _bookingRepository.GetById(bookingId);

        return booking?.ToApi();
    }

    public async Task<BookingDto> CreateBookingAsync(Guid eventId)
    {

        var booking = await _bookingRepository.Add(eventId);

        return booking.ToApi();
    }
}
