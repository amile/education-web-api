using Users.Domain;

namespace Users.Application;

public interface IJWTTokenService
{
    Task<TokenResultDto> GenerateToken(User user, CancellationToken cancellationToken = default);
}
