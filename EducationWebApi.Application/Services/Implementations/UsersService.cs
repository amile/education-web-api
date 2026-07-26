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
            throw new UserNotFoundExeption();
        }

        var token = await _jwtTokenService.GenerateToken(userRequest.Login);

        return token;
    }

    public async Task<TokenResultDto> RegisterUserAsync(RegisterUserRequestDto userRequest, CancellationToken cancellationToken = default)
    {
        await _usersRepository.AddUserAsync(new User(userRequest.Login, userRequest.Password, UserRole.User), cancellationToken);
        await _usersRepository.SaveChangesAsync(cancellationToken);

        var token = await _jwtTokenService.GenerateToken(userRequest.Login);

        return token;
    }
}
