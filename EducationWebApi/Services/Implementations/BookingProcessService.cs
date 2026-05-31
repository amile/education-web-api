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
            try
            {
                var pending = await _bookingRepository.GetPending();

                if (pending is not null)
                {
                    _logger.LogInformation("Start booking event id: {id}", pending.Id);

                    await Task.Delay(TimeSpan.FromSeconds(2), ct);

                    pending.Status = BookingStatus.Confirmed;
                    pending.ProcessedAt = DateTime.UtcNow;

                    await _bookingRepository.TryUpdate(pending);

                    _logger.LogInformation("Booking event id: {id} succeeded", pending.Id);
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Booking event error");
            }

            await Task.Delay(TimeSpan.FromSeconds(4), ct);
        }
    }
}