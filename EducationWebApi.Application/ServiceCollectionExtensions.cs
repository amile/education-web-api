using Microsoft.Extensions.DependencyInjection;

namespace EducationWebApi.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection sc)
    {
        sc.AddScoped<IEventsService, EventsService>();
        sc.AddScoped<IBookingService, BookingService>();

        sc.AddHostedService<BookingProcessService>();

        return sc;
    }
}