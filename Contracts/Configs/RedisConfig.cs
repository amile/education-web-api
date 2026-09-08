namespace Contracts;

public class RedisConfig
{
    public required string ConnectionString { get; set; }
    public required int ConnectTimeout { get; set; } = 5000;
    public required int SyncTimeout { get; set; } = 3000;
    public required bool AbortOnConnectFail { get; set; } = false;
    public required int ConnectRetry { get; set; } = 3;

    public RedisConfig()
    {}

    public RedisConfig(string endPoint)
    {
        ConnectionString = endPoint;
    }
}
