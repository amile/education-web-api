using EducationWebApi.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EducationWebApi.Infrastructure;

public static class ServiceCollectionExtensions
{
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
        sc.AddScoped<IUsersRepository, UsersRepository>();
        sc.AddScoped<IEventsRepository, EventsRepository>();
        sc.AddScoped<IBookingRepository, BookingRepository>();

        return sc;
    }
}