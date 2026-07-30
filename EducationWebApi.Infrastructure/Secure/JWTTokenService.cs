using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EducationWebApi.Application;
using EducationWebApi.Domain;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EducationWebApi.Infrastructure.Secure;

public class JWTTokenService : IJWTTokenService
{
     private readonly JWTTokenConfig _jwtTokenConfig;
    public JWTTokenService(IOptions<JWTTokenConfig> options)
    {
        _jwtTokenConfig = options.Value;
    }

    public async Task<TokenResultDto> GenerateToken(User user, CancellationToken cancellationToken = default)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Login),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtTokenConfig.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtTokenConfig.Issuer,
            audience: _jwtTokenConfig.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtTokenConfig.ExpiresMinutes),
            signingCredentials: creds
        );

        string accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new TokenResultDto(accessToken);
    }
}
