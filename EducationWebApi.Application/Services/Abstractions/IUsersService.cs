namespace EducationWebApi.Application;

public interface IUsersService
{
    Task<TokenResultDto> RegisterUserAsync(RegisterUserRequestDto user, CancellationToken cancellationToken = default);
    Task<TokenResultDto> LoginUserAsync(LoginUserRequestDto user, CancellationToken cancellationToken = default);
}
