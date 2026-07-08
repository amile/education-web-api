using EducationWebApi;

public class BookingProcessService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingProcessService> _logger;
    private readonly SemaphoreSlim _processingSemaphore = new(1, 1);

    private const int PollingInterval = 4;
    private const int ProcessingDelay = 2;

    public BookingProcessService(
        IServiceScopeFactory scopeFactory,
        ILogger<BookingProcessService> logger
    )
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

            List<Booking> pendingBookings = await bookingRepository.GetPendingBookingsAsync(ct);

            var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking, ct));
            await Task.WhenAll(tasks);

            await Task.Delay(TimeSpan.FromSeconds(PollingInterval), ct);
        }
    }

    private async Task ProcessBookingAsync(Booking booking, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var eventsRepository = scope.ServiceProvider.GetRequiredService<IEventsRepository>();
        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

        var succeeded = false;
        Event? eventItem = null;

        try
        {
            _logger.LogInformation("Start booking event id: {id}", booking.Id);

            await Task.Delay(TimeSpan.FromSeconds(ProcessingDelay), ct);

            await _processingSemaphore.WaitAsync();

            eventItem = await eventsRepository.GetEventByIdAsync(booking.EventId, ct);

            if (eventItem is not null)
            {
                await bookingRepository.ConfirmBookingAsync(booking.Id, ct);
                await bookingRepository.SaveChangesAsync(ct);
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
            if (!succeeded)
            {
                await bookingRepository.RejectBookingAsync(booking.Id);
                await bookingRepository.SaveChangesAsync(ct);

                if (eventItem is not null)
                {
                    eventItem.ReleaseSeats();
                    await eventsRepository.ChangeEventAsync(eventItem, ct);
                }
            }

            _processingSemaphore.Release();
        } 
    }
}