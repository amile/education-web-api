namespace EducationWebApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainServices(this IServiceCollection sc)
    {
        sc.AddScoped<IEventsService, EventsService>();

        sc.AddScoped<IBookingRepository, BookingRepository>();
        sc.AddScoped<IBookingService, BookingService>();

        return sc;
    }
}