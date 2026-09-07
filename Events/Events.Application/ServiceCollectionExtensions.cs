using Events.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Events.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection sc, IConfiguration configuration)
    {
        sc.Configure<EventsCacheConfig>(configuration.GetSection(EventsCacheConstants.EventsCacheSectionName));
        
        sc.AddScoped<IEventsService, EventsService>();

        return sc;
    }
}