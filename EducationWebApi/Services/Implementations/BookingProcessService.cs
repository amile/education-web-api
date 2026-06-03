using EducationWebApi;

public class BookingProcessService : BackgroundService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ILogger<BookingProcessService> _logger;

    public BookingProcessService(
        IBookingRepository bookingRepository,
        ILogger<BookingProcessService> logger)
    {
        _bookingRepository = bookingRepository;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var bookings = await _bookingRepository.GetAllPendingBookings();

            foreach (var item in bookings)
            {
                await BookEvent(item, ct);
            }

            await Task.Delay(TimeSpan.FromSeconds(4), ct);
        }
    }

    private async Task BookEvent(Booking booking, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Start booking event id: {id}", booking.Id);

            await Task.Delay(TimeSpan.FromSeconds(2), ct);

            await _bookingRepository.ConfirmBooking(booking.Id);

            _logger.LogInformation("Booking event id: {id} succeeded", booking.Id);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _logger.LogInformation("Booking event cancelled");
        }
        catch (Exception ex)
        {
            await _bookingRepository.RejectBooking(booking.Id);
            _logger.LogError(ex, "Booking event error");
        }
    }
}