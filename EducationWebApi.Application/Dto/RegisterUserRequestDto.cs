using EducationWebApi.Domain;

namespace EducationWebApi.Application;

public record RegisterUserRequestDto(
    string Login,
    string Password,
    UserRole? Role = UserRole.User
);
