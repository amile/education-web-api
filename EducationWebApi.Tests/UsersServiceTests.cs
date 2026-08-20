using EducationWebApi.Application;
using EducationWebApi.Domain;
using EducationWebApi.Infrastructure;
using EducationWebApi.Infrastructure.Secure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EducationWebApi.Tests;

public class UsersServiceTests
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IServiceScope _scope;
    private readonly IUsersService _usersService;

    public UsersServiceTests()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Token:Issuer"] = "EducationWebApi",
                ["Token:Audience"] = "EducationWebApi",
                ["Token:ExpiresMinutes"] = "30",
                ["Token:Secret"] = "It55XR94hJ520UoGZR2jt3VPUIQ1xKfb1KyDKLlK4HG"
            })
            .Build();
        var tokenConfigSection = configuration.GetSection("Token");
        services.Configure<JWTTokenConfig>(tokenConfigSection);
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<IJWTTokenService, JWTTokenService>();
        services.AddScoped<IUsersService, UsersService>();

        _serviceProvider = services.BuildServiceProvider();
        _scope = _serviceProvider.CreateScope();
        _usersService = _scope.ServiceProvider.GetRequiredService<IUsersService>();
    }

    [Fact]
    public async Task RegisterUser_Ok()
    {
        //Arrange
        var newUser = new RegisterUserRequestDto("user", "password");

        //Act
        var token = await _usersService.RegisterUserAsync(newUser);

        //Assert
        Assert.NotNull(token);
        Assert.NotNull(token.AccessToken);
    }

    [Fact]
    public async Task LoginUser_Ok()
    {
        //Arrange
        var newUser = new RegisterUserRequestDto("user", "password");
        await _usersService.RegisterUserAsync(newUser);

        //Act
        var token = await _usersService.LoginUserAsync(new LoginUserRequestDto("user", "password"));

        //Assert
        Assert.NotNull(token);
        Assert.NotNull(token.AccessToken);
    }

    [Fact]
    public async Task LoginUser_WrongLogin()
    {
        //Arrange
        var newUser = new RegisterUserRequestDto("user", "password");
        await _usersService.RegisterUserAsync(newUser);

        //Assert
        var loginUser = new LoginUserRequestDto("user1", "password");
        var error = await Assert.ThrowsAsync<InvalidCredentialsException>(() => _usersService.LoginUserAsync(loginUser));
        Assert.Equal("Incorrect login or password", error.Message);
    }

    [Fact]
    public async Task LoginUser_WrongPassword()
    {
        //Arrange
        var newUser = new RegisterUserRequestDto("user", "password");
        await _usersService.RegisterUserAsync(newUser);

        //Assert
        var loginUser = new LoginUserRequestDto("user", "password1");
        var error = await Assert.ThrowsAsync<InvalidCredentialsException>(() => _usersService.LoginUserAsync(loginUser));
        Assert.Equal("Incorrect login or password", error.Message);
    }
}
