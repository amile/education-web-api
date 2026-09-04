using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class KafkaInitializerService : IHostedService
{
    private readonly ILogger<KafkaInitializerService> _logger;
    private readonly string _bootstrapServers;

    public KafkaInitializerService(
        IOptions<KafkaConfig> options,
        ILogger<KafkaInitializerService> logger
    )
    {
        _logger = logger;
        
        var kafkaConfig = options.Value;
        _bootstrapServers = kafkaConfig.BootstrapServers; 
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var config = new AdminClientConfig { BootstrapServers = _bootstrapServers };
        using var adminClient = new AdminClientBuilder(config).Build();

        var topicSpec = new TopicSpecification
        {
            Name = KafkaConstants.BookingConfirmedTopicName,
            NumPartitions = 1,
        };

        try
        {
            await adminClient.CreateTopicsAsync(new[] { topicSpec });
            _logger.LogInformation("Topic '{topic}' created successfully", topicSpec.Name);
        }
        catch (CreateTopicsException e)
        {
            foreach (var result in e.Results)
            {
                if (result.Error.Code == ErrorCode.TopicAlreadyExists)
                {
                    _logger.LogInformation("Topic {topic} already exists", result.Topic);
                }
                else
                {
                    _logger.LogError("Topic {topic} creation failed: {reason}", result.Topic, result.Error.Reason);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
