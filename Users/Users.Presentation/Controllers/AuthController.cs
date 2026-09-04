using Microsoft.AspNetCore.Mvc;
using Users.Application;

namespace Users.Presentation;

[ApiController]
[Route("[controller]")]

public class AuthController : ControllerBase
{
    private readonly IUsersService _usersService;

    public AuthController(IUsersService usersService)
    {
        _usersService = usersService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequestDto request, CancellationToken ct)
    {
        var token = await _usersService.RegisterUserAsync(request, ct);

        return Ok(token);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserRequestDto request, CancellationToken ct)
    {
        var token = await _usersService.LoginUserAsync(request, ct);

        return Ok(token);
    }
}