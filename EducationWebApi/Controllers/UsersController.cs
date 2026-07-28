using EducationWebApi.Application;
using Microsoft.AspNetCore.Mvc;

namespace EducationWebApi;

[ApiController]
[Route("[controller]")]

public class UsersController : ControllerBase
{
    private readonly IUsersService _usersService;

    public UsersController(IUsersService usersService)
    {
        _usersService = usersService;
    }

    [HttpPost("auth/register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequestDto request, CancellationToken ct)
    {
        var token = await _usersService.RegisterUserAsync(request, ct);

        return Ok(token);
    }

    [HttpPost("auth/login")]
    public async Task<IActionResult> Login([FromBody] LoginUserRequestDto request, CancellationToken ct)
    {
        var token = await _usersService.LoginUserAsync(request, ct);

        return Ok(token);
    }
}