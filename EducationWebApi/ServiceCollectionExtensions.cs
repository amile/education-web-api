namespace EducationWebApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainServices(this IServiceCollection sc)
    {
        sc.AddScoped<IEventsService, EventsService>();

        sc.AddSingleton<IBookingRepository, BookingRepository>();
        sc.AddHostedService<BookingProcessService>();
        sc.AddScoped<IBookingService, BookingService>();

        return sc;
    }
}