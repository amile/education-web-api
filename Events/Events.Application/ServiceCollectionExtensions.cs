using Microsoft.Extensions.DependencyInjection;

namespace Events.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection sc)
    {
        sc.AddScoped<IEventsService, EventsService>();

        return sc;
    }
}