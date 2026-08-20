using EducationWebApi.Application.Helpers;
using EducationWebApi.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationWebApi.Infrastructure;

public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("users");

        builder.HasKey(b => b.Id);
        
        builder.Property(e => e.Id).ValueGeneratedNever();;

        builder.Property(b => b.Login).HasMaxLength(200);

        builder.HasIndex(u => u.Login).IsUnique();

        builder.HasMany(e => e.Bookings)
            .WithOne(b => b.User)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(new UserEntity
        { 
            Id = Guid.Parse("5b7d895e-e776-49b2-880a-22435b255267"), 
            Login = "admin", 
            PasswordHash = "AQAAAAIAAYagAAAAEO9eG0aVlSAQZ8ulAGO1BsAF12ST745iOvUKlBDpP5yWKONDf9H+yJbhiZL9OUGbuA==", 
            Role = "Admin",
        });
    }
}