namespace EducationWebApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainServices(this IServiceCollection sc)
    {
        sc.AddScoped<IEventsRepository, EventsRepository>();
        sc.AddScoped<IEventsService, EventsService>();

        sc.AddScoped<IBookingRepository, BookingRepository>();
        sc.AddScoped<IBookingService, BookingService>();

        sc.AddHostedService<BookingProcessService>();

        return sc;
    }
}