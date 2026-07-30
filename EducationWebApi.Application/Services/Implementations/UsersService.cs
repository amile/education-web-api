using EducationWebApi.Application.Helpers;
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

        if (user is null || !HashDataHelper.Verify(userRequest.Login, userRequest.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        var token = await _jwtTokenService.GenerateToken(user);

        return token;
    }

    public async Task<TokenResultDto> RegisterUserAsync(RegisterUserRequestDto userRequest, CancellationToken cancellationToken = default)
    {
        var passwordHash = HashDataHelper.Hash(userRequest.Login, userRequest.Password);
        var user = new User(userRequest.Login, passwordHash, UserRole.User);
        await _usersRepository.AddUserAsync(user, cancellationToken);
        await _usersRepository.SaveChangesAsync(cancellationToken);

        var token = await _jwtTokenService.GenerateToken(user);

        return token;
    }
}
