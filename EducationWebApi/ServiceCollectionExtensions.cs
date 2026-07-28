using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace EducationWebApi.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuth(this IServiceCollection sc, IConfiguration configuration)
    {
        var tokenConfigSection = configuration.GetSection("token");
        var tokenConfig = tokenConfigSection.Get<JWTTokenConfig>() ?? throw new ArgumentNullException("Token config section is empty");
        sc.Configure<JWTTokenConfig>(tokenConfigSection);

        sc.AddAuthentication(options => 
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options => 
        {
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuer = tokenConfig.Issuer,

                ValidateAudience = true,
                ValidAudience = tokenConfig.Audience,

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,

                ValidateIssuerSigningKey = true,

                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenConfig.Secret)),
            };
        });
        sc.AddAuthorization();

        return sc;
    }
}