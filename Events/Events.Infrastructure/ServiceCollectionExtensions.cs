using Contracts;
using Events.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Events.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection sc, IConfiguration configuration)
    {
        AddDataAccess(sc, configuration);
        AddRepositories(sc);
        AddKafkaConsumer(sc, configuration);
        AddCache(sc, configuration);

        return sc;
    }
    public static IServiceCollection AddDataAccess(this IServiceCollection sc, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("Connection string 'Default' not found.");

        sc.AddDbContext<AppDbContext>(options => 
            options.UseNpgsql(connectionString)
                // .LogTo(Console.WriteLine, LogLevel.Information)
                // .EnableDetailedErrors() 
        );

        return sc;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection sc)
    {
        sc.AddScoped<IEventsRepository, EventsRepository>();

        return sc;
    }

    public static IServiceCollection AddKafkaConsumer(this IServiceCollection sc, IConfiguration configuration)
    {
        var kafkaConfigSection = configuration.GetSection(KafkaConstants.KafkaSettingsSectionName);
        var kafkaConfig = kafkaConfigSection.Get<KafkaConfig>() ?? throw new ArgumentNullException("Kafka config section is empty");
        sc.Configure<KafkaConfig>(kafkaConfigSection);

        sc.AddHostedService<KafkaInitializerService>();
        sc.AddHostedService<BookingConsumerService>();

        return sc;
    }

    public static IServiceCollection AddCache(this IServiceCollection sc, IConfiguration configuration)
    {
        var redisConfigSection = configuration.GetSection(RedisConstants.RedisSettingsSectionName);
        var redisConfig = redisConfigSection.Get<RedisConfig>() ?? throw new ArgumentNullException("Redis config section is empty");
        sc.Configure<RedisConfig>(redisConfigSection);

        sc.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(new ConfigurationOptions
            {
                EndPoints = { redisConfig.ConnectionString },
                ConnectTimeout = redisConfig.ConnectRetry,
                SyncTimeout = redisConfig.SyncTimeout,
                AbortOnConnectFail = redisConfig.AbortOnConnectFail,
                ConnectRetry = redisConfig.ConnectRetry,
            }
        ));

        sc.AddSingleton<ICacheService, RedisCacheService>();

        return sc;
    }
}