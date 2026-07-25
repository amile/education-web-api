using Microsoft.EntityFrameworkCore;

namespace EducationWebApi.Infrastructure;

public class AppDbContext : DbContext
{
    public DbSet<EventEntity> Events { get; set; }
    public DbSet<BookingEntity> Bookings { get; set; }
    public DbSet<UserEntity> Users { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
} 
