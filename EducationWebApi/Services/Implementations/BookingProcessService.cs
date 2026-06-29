using EducationWebApi;

public class BookingProcessService : BackgroundService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IEventsRepository _eventsRepository;
    private readonly ILogger<BookingProcessService> _logger;
    private readonly SemaphoreSlim _processingSemaphore = new(1, 1);

    private const int PollingInterval = 4;

    private const int ProcessingDelay = 2;

    public BookingProcessService(
        IBookingRepository bookingRepository,
        IEventsRepository eventsRepository,
        ILogger<BookingProcessService> logger
    )
    {
        _bookingRepository = bookingRepository;
        _eventsRepository = eventsRepository;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var pendingBookings = _bookingRepository.GetAllPendingBookings();
            var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking, ct));
            await Task.WhenAll(tasks);

            await Task.Delay(TimeSpan.FromSeconds(PollingInterval), ct);
        }
    }

    private async Task ProcessBookingAsync(Booking booking, CancellationToken ct)
    {
        var succeeded = false;
        Event? eventItem = null;

        try
        {
            _logger.LogInformation("Start booking event id: {id}", booking.Id);

            await Task.Delay(TimeSpan.FromSeconds(ProcessingDelay), ct);

            await _processingSemaphore.WaitAsync();

            if(_eventsRepository.TryGetEvent(booking.EventId, out eventItem))
            {
                _bookingRepository.ConfirmBooking(booking.Id);
                succeeded = true;
                _logger.LogInformation("Booking event id: {id} succeeded", booking.Id);
            }
            else
            {
                _logger.LogWarning("Event id: {eventId} not found", booking.EventId);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _logger.LogInformation("Booking event cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Booking event error");
        }
        finally
        {
            _processingSemaphore.Release();

            if (!succeeded)
            {
                _bookingRepository.RejectBooking(booking.Id);

                if (eventItem is not null)
                {
                    eventItem.ReleaseSeats();
                    _eventsRepository.TryChangeEvent(eventItem);
                }
            }
        } 
    }
}