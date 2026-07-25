namespace EducationWebApi.Infrastructure;

public class UserEntity
{
    public required Guid Id { get; set; }
    public required string Login { get; set; }
    public required string PasswordHash { get; set; }
    public required string Role { get; set; }
    public List<BookingEntity>? Bookings { get; set; }
}