using EducationWebApi.Domain;

namespace EducationWebApi.Infrastructure;

public class UserFactory
{
    public static User FromDb(UserEntity dbModel) => new User(dbModel.Id, dbModel.Login, dbModel.PasswordHash, Enum.Parse<UserRole>(dbModel.Role));

    public static UserEntity ToDb(User model) => new UserEntity()
    {
        Id = model.Id,
        Login = model.Login,
        PasswordHash = model.PasswordHash,
        Role = model.Role.ToString(),
    };
}