using Microsoft.EntityFrameworkCore;

namespace Events.Infrastructure;

public class AppDbContext : DbContext
{
    public DbSet<EventEntity> Events { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
} 
