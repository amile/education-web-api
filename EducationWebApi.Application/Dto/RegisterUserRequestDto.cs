namespace EducationWebApi.Application;

public record RegisterUserRequestDto(
    string Login,
    string Password
);
