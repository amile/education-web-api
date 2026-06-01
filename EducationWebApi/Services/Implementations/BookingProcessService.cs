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
            var pending = await _bookingRepository.GetPending();

            try
            {
                if (pending is not null)
                {
                    _logger.LogInformation("Start booking event id: {id}", pending.Id);

                    await Task.Delay(TimeSpan.FromSeconds(2), ct);

                    pending.Status = BookingStatus.Confirmed;

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
            finally
            {
                if (pending is not null)
                {
                    if (pending.Status != BookingStatus.Confirmed)
                    {
                        pending.Status = BookingStatus.Rejected;
                    }

                    pending.ProcessedAt = DateTime.UtcNow;
                    await _bookingRepository.TryUpdate(pending);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(4), ct);
        }
    }
}