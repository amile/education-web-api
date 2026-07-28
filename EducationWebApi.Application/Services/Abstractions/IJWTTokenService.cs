using EducationWebApi.Domain;

namespace EducationWebApi.Application;

public interface IJWTTokenService
{
    Task<TokenResultDto> GenerateToken(User user, CancellationToken cancellationToken = default);
}
