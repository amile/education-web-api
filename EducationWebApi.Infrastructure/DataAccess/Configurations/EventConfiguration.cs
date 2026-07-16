using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationWebApi.Infrastructure;

public class EventConfiguration : IEntityTypeConfiguration<EventEntity>
{
    public void Configure(EntityTypeBuilder<EventEntity> builder)
    {
        builder.ToTable("events");

        builder.HasKey(b => b.Id);
        
        builder.Property(e => e.Id).ValueGeneratedNever();;

        builder.Property(b => b.Title).HasMaxLength(200);

        builder.HasMany(e => e.Bookings)
            .WithOne(b => b.Event)
            .HasForeignKey(b => b.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}