using Bookings.Domain;
using Confluent.Kafka;
using Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Bookings.Application;

public class BookingProviderService : IBookingProviderService, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<BookingProviderService> _logger;

    public BookingProviderService(IOptions<KafkaConfig> options, ILogger<BookingProviderService> logger)
    {
        var kafkaConfig = options.Value;
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = kafkaConfig.BootstrapServers,
            Acks = Acks.All,
        };

        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
        _logger = logger;
    }

    public async Task Publish(Booking booking, CancellationToken cancellationToken)
    {
        var bookingConfirmed = new BookingConfirmed(
            booking.Id,
            booking.EventId,
            booking.UserId,
            1,
            DateTime.UtcNow
        );

        var result = await _producer.ProduceAsync(KafkaConstants.BookingConfirmedTopicName, new Message<string, string>
        {
            Key = bookingConfirmed.EventId.ToString(),
            Value = JsonSerializer.Serialize(bookingConfirmed)
        }, cancellationToken);

        _logger.LogInformation("Publish booking {BookingId} to confirm", bookingConfirmed.BookingId);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}
