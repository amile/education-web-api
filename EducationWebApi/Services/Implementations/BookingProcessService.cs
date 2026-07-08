using EducationWebApi;
using EducationWebApi.DAL;
using Microsoft.EntityFrameworkCore;

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
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            List<Booking> pendingBookings = await dbContext.Bookings
                .Where(item => item.Status == BookingStatus.Pending.ToString())
                .Select(item => Booking.FromDb(item))
                .ToListAsync();

            var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking, ct));
            await Task.WhenAll(tasks);

            await Task.Delay(TimeSpan.FromSeconds(PollingInterval), ct);
        }
    }

    private async Task ProcessBookingAsync(Booking booking, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var eventsRepository = scope.ServiceProvider.GetRequiredService<IEventsRepository>();
        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

        var succeeded = false;
        Event? eventItem = null;

        try
        {
            _logger.LogInformation("Start booking event id: {id}", booking.Id);

            await Task.Delay(TimeSpan.FromSeconds(ProcessingDelay), ct);

            await _processingSemaphore.WaitAsync();

            eventItem = await eventsRepository.TryGetEvent(booking.EventId);

            if (eventItem is not null)
            {
                await bookingRepository.ConfirmBooking(booking.Id);
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
                await bookingRepository.RejectBooking(booking.Id);

                if (eventItem is not null)
                {
                    eventItem.ReleaseSeats();
                    await eventsRepository.TryChangeEvent(eventItem);
                }
            }

            await dbContext.SaveChangesAsync();

            _processingSemaphore.Release();
        } 
    }
}