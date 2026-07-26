using EducationWebApi.Domain;

namespace EducationWebApi.Application;

public record LoginUserRequestDto(
    string Login,
    string Password,
    UserRole Role
);
