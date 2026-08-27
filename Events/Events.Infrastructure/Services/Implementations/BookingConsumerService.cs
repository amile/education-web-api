using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Contracts;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Events.Application;
using Microsoft.Extensions.Options;

namespace Events.Infrastructure;

public class BookingConsumerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<BookingConsumerService> _logger;

    private const string ConsumerGroupId = "booking-processing";

    public BookingConsumerService(
        IOptions<KafkaConfig> options,
        IServiceScopeFactory scopeFactory,
        ILogger<BookingConsumerService> logger
    )
    {
        _scopeFactory = scopeFactory;
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

    private async void Consume(CancellationToken cancellationToken)
    {
        _consumer.Subscribe(KafkaConstants.BookingConfirmedTopicName);
        using var scope = _scopeFactory.CreateScope();
        var eventsRepository = scope.ServiceProvider.GetRequiredService<IEventsRepository>();

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("BookingConsumerService while {time}", DateTime.UtcNow);

                var consumeResult = _consumer.Consume(cancellationToken);

                var booking = JsonSerializer.Deserialize<BookingConfirmed>(consumeResult.Message.Value);

                if (booking is null)
                {
                    _logger.LogWarning("Booking deserialize error");
                    continue;
                }

                var eventItem = await eventsRepository.GetEventByIdAsync(booking.EventId, cancellationToken);

                if (eventItem is null)
                {
                    _logger.LogWarning("Event id: {eventId} not found", booking.EventId);
                    continue;
                }

                if (eventItem.AlreadyStarted())
                {
                    _logger.LogWarning("Event id: {eventId} already started", booking.EventId);
                    continue;
                }

                if (!eventItem.TryReserveSeats())
                {
                    _logger.LogWarning("Event id: {eventId} has no available seats", booking.EventId);
                    continue;
                }

                await eventsRepository.ChangeEventAsync(eventItem, cancellationToken);
                await eventsRepository.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Booking event id: {eventId} succeeded", booking.EventId);
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

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        base.Dispose();
    }
}
