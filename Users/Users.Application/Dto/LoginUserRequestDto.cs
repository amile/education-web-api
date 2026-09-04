using Users.Domain;

namespace Users.Application;

public record LoginUserRequestDto(
    string Login,
    string Password
);
