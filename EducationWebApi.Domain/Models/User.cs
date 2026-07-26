using System.Diagnostics.CodeAnalysis;

namespace EducationWebApi.Domain;

public class User
{
    public required Guid Id { get; set; }
    public required string Login { get; set; }
    public required string PasswordHash { get; set; }
    public required UserRole Role { get; set; }

    [SetsRequiredMembers]
    public User(Guid id, string login, string passwordHash, UserRole role)
    {
        Id = id;
        Login = login;
        PasswordHash = passwordHash;
        Role = role;
    }

    [SetsRequiredMembers]
    public User(string login, string passwordHash, UserRole role) : this(Guid.NewGuid(), login, passwordHash, role) {}
}