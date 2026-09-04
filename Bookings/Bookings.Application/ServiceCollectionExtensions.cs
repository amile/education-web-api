using Microsoft.Extensions.DependencyInjection;

namespace Bookings.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection sc)
    {
        sc.AddScoped<IBookingService, BookingService>();

        return sc;
    }
}