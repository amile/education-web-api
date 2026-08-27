namespace Contracts;

public class KafkaConfig
{
    public required string BootstrapServers { get; set; }

    public KafkaConfig()
    {}

    public KafkaConfig(string bootstrapServers)
    {
        BootstrapServers = bootstrapServers;
    }
}
