namespace EducationWebApi.Application;

public interface IJWTTokenService
{
    Task<TokenResultDto> GenerateToken(string userName, CancellationToken cancellationToken = default);
}
