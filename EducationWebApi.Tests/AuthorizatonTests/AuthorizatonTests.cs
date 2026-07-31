using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace EducationWebApi.Tests;

public class AuthorizationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthorizationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_SecureEndpoint_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/admin-health");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_SecureEndpoint_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication(options =>
                        {
                            options.DefaultAuthenticateScheme = TestAuthUserHandler.AuthenticationScheme;
                            options.DefaultChallengeScheme = TestAuthUserHandler.AuthenticationScheme;
                        })
                        .AddScheme<AuthenticationSchemeOptions, TestAuthUserHandler>(
                            TestAuthUserHandler.AuthenticationScheme, options => { });
                });
            })
            .CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: TestAuthUserHandler.AuthenticationScheme);

        //Act
        var response = await client.GetAsync("/admin-health");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Get_SecureEndpoint_ReturnsOk()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication(options =>
                        {
                            options.DefaultAuthenticateScheme = TestAuthAdminHandler.AuthenticationScheme;
                            options.DefaultChallengeScheme = TestAuthAdminHandler.AuthenticationScheme;
                        })
                        .AddScheme<AuthenticationSchemeOptions, TestAuthAdminHandler>(
                            TestAuthAdminHandler.AuthenticationScheme, options => { });
                });
            })
            .CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: TestAuthAdminHandler.AuthenticationScheme);

        //Act
        var response = await client.GetAsync("/admin-health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
