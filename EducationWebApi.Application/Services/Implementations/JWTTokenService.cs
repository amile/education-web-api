using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EducationWebApi.Application;

public class JWTTokenService : IJWTTokenService
{
     private readonly JWTTokenConfig _jwtTokenConfig;
    public JWTTokenService(IOptions<JWTTokenConfig> options)
    {
        _jwtTokenConfig = options.Value;
    }

    public async Task<TokenResultDto> GenerateToken(string userName, CancellationToken cancellationToken = default)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, userName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtTokenConfig.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtTokenConfig.Issuer,
            audience: _jwtTokenConfig.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_jwtTokenConfig.ExpiresMinutes),
            signingCredentials: creds
        );

        string accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new TokenResultDto(accessToken);
    }
}
