using EducationWebApi.Domain;

namespace EducationWebApi.Application;

public class UsersService : IUsersService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IJWTTokenService _jwtTokenService;

    public UsersService(
        IUsersRepository usersRepository,
        IJWTTokenService jwtTokenService
    )
    {
        _usersRepository = usersRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<TokenResultDto> LoginUserAsync(LoginUserRequestDto userRequest, CancellationToken cancellationToken = default)
    {
        var user = await _usersRepository.GetUserByLoginAsync(userRequest.Login, cancellationToken);

        if (user is null || HashDataHelper.GetHash(userRequest.Password) != user.PasswordHash)
        {
            throw new UnauthorizedAccessException($"Incorrect login or password");
        }

        var token = await _jwtTokenService.GenerateToken(user);

        return token;
    }

    public async Task<TokenResultDto> RegisterUserAsync(RegisterUserRequestDto userRequest, CancellationToken cancellationToken = default)
    {
        var user = new User(userRequest.Login, HashDataHelper.GetHash(userRequest.Password), userRequest.Role ?? UserRole.User);
        await _usersRepository.AddUserAsync(user, cancellationToken);
        await _usersRepository.SaveChangesAsync(cancellationToken);

        var token = await _jwtTokenService.GenerateToken(user);

        return token;
    }
}
