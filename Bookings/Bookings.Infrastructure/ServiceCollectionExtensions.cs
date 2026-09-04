using Bookings.Application;
using Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bookings.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection sc, IConfiguration configuration)
    {
        AddDataAccess(sc, configuration);
        AddRepositories(sc);
        AddKafkaProducer(sc, configuration);

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
        sc.AddScoped<IBookingRepository, BookingRepository>();

        return sc;
    }

    public static IServiceCollection AddKafkaProducer(this IServiceCollection sc, IConfiguration configuration)
    {
        var kafkaConfigSection = configuration.GetSection(KafkaConstants.KafkaSettingsSectionName);
        var kafkaConfig = kafkaConfigSection.Get<KafkaConfig>() ?? throw new ArgumentNullException("Kafka config section is empty"); 
        sc.Configure<KafkaConfig>(kafkaConfigSection);

        sc.AddSingleton<IBookingProviderService, BookingProviderService>();

        return sc;
    }
}