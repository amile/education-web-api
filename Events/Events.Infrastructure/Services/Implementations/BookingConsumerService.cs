using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Contracts;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Events.Application;
using Microsoft.Extensions.Options;
using Events.Domain;

namespace Events.Infrastructure;

public class BookingConsumerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConsumer<string, string> _consumer;
     private readonly ICacheService _cache;
    private readonly ILogger<BookingConsumerService> _logger;

    private const string ConsumerGroupId = "booking-processing";

    public BookingConsumerService(
        IOptions<KafkaConfig> options,
        IServiceScopeFactory scopeFactory,
        ICacheService cache,
        ILogger<BookingConsumerService> logger
    )
    {
        _scopeFactory = scopeFactory;
        _cache = cache;
        _logger = logger;

        var kafkaConfig = options.Value;
        var config = new ConsumerConfig
        {
            BootstrapServers = kafkaConfig.BootstrapServers,
            GroupId = ConsumerGroupId,
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        return Task.Run(() => Consume(cancellationToken), cancellationToken);
    }

    private async Task Consume(CancellationToken cancellationToken)
    {
        _consumer.Subscribe(KafkaConstants.BookingConfirmedTopicName);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ConsumeResult<string, string>? consumeResult;

                try
                {
                    consumeResult = _consumer.Consume(cancellationToken);;
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error while consuming from Kafka");
                    continue;
                }

                if (consumeResult is null)
                {
                    _logger.LogWarning("Consume result from Kafka is null");
                    continue;
                }

                await HandleConsumeResultAsync(consumeResult, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _consumer.Close();
        }
    }

    
    private async Task HandleConsumeResultAsync(ConsumeResult<string, string> consumeResult, CancellationToken cancellationToken)
    {
        BookingConfirmed? booking;
        try
        {
            booking = JsonSerializer.Deserialize<BookingConfirmed>(consumeResult.Message.Value);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Booking deserialize error");
            return;
        }

        if (booking is null)
        {
            _logger.LogWarning("Booking deserialize result is null");
            return;
        }  

        using var scope = _scopeFactory.CreateScope();
        var eventsRepository = scope.ServiceProvider.GetRequiredService<IEventsRepository>();

        var eventItem = await eventsRepository.GetEventByIdAsync(booking.EventId, cancellationToken);

        if (eventItem is null)
        {
            _logger.LogWarning("Event id: {eventId} not found", booking.EventId);
            return;
        }

        if (eventItem.AlreadyStarted())
        {
            _logger.LogWarning("Event id: {eventId} already started", booking.EventId);
            return;
        }

        if (!eventItem.TryReserveSeats())
        {
            _logger.LogWarning("Event id: {eventId} has no available seats", booking.EventId);
            return;
        }

        await eventsRepository.ChangeEventAsync(eventItem, cancellationToken);
        await eventsRepository.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(EventsCacheConstants.EventKey(booking.EventId), cancellationToken);
        await _cache.RemoveAsync(EventsCacheConstants.TopEventsKey, cancellationToken);

        _logger.LogInformation("Booking event id: {eventId} succeeded", booking.EventId);
    }

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        base.Dispose();
    }
}
