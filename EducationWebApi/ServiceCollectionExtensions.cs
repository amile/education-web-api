namespace EducationWebApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainServices(this IServiceCollection sc)
    {
        sc.AddScoped<IEventsService, EventsService>();

        return sc;
    }
}