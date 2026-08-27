using Microsoft.EntityFrameworkCore;

namespace Bookings.Infrastructure;

public class AppDbContext : DbContext
{
    public DbSet<BookingEntity> Bookings { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
} 
