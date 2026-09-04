using Microsoft.Extensions.DependencyInjection;

namespace Users.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection sc)
    {
        sc.AddScoped<IUsersService, UsersService>();

        return sc;
    }
}