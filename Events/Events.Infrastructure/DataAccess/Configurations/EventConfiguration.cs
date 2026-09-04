using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Events.Infrastructure;

public class EventConfiguration : IEntityTypeConfiguration<EventEntity>
{
    public void Configure(EntityTypeBuilder<EventEntity> builder)
    {
        builder.ToTable("events");

        builder.HasKey(b => b.Id);
        
        builder.Property(e => e.Id).ValueGeneratedNever();;

        builder.Property(b => b.Title).HasMaxLength(200);
    }
}