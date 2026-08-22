using Users.Domain;
using Users.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Users.Tests;

[Collection("RepositoryTestCollection")]
public class UsersRepositoryTests
{
    readonly RepositoryTestFixture _dbFixture;

    public UsersRepositoryTests(RepositoryTestFixture dbFixture)
    {
        _dbFixture = dbFixture;
    }

    [Fact]
    public async Task AddUser_Ok()
    {
        //Arrange
        await _dbFixture.ResetDatabaseAsync();
        await using var context = _dbFixture.CreateContext();
        var repository = new UsersRepository(context);

        var user = new User(Guid.NewGuid(), "User", "Password", UserRole.User);

        //Act
        await repository.AddUserAsync(user);
        await repository.SaveChangesAsync();

        //Assert
        await using var verifyContext = _dbFixture.CreateContext();
        var actualUser = await verifyContext.Users.FirstOrDefaultAsync(b => b.Id == user.Id);
        Assert.NotNull(actualUser);
        Assert.Equal(user.Id, user.Id);
        Assert.Equal(user.Login, actualUser.Login);
        Assert.Equal(user.Role.ToString(), actualUser.Role);
    }

    [Fact]
    public async Task GetUserById_Ok()
    {
        //Arrange
        await _dbFixture.ResetDatabaseAsync();
        await using var context = _dbFixture.CreateContext();
        var repository = new UsersRepository(context);

        var user = new User(Guid.NewGuid(), "User", "Password", UserRole.User);
        await repository.AddUserAsync(user);
        await repository.SaveChangesAsync();

        //Act
        var actualUser = await repository.GetUserByIdAsync(user.Id);

        //Assert
        Assert.NotNull(actualUser);
        Assert.Equal(user.Id, actualUser.Id);
        Assert.Equal(user.Login, actualUser.Login);
        Assert.Equal(user.Role, actualUser.Role);
    }

    [Fact]
    public async Task GetUserByLogin_Ok()
    {
        //Arrange
        await _dbFixture.ResetDatabaseAsync();
        await using var context = _dbFixture.CreateContext();
        var repository = new UsersRepository(context);

        var user = new User(Guid.NewGuid(), "User", "Password", UserRole.User);
        await repository.AddUserAsync(user);
        await repository.SaveChangesAsync();

        //Act
        var actualUser = await repository.GetUserByLoginAsync(user.Login);

        //Assert
        Assert.NotNull(actualUser);
        Assert.Equal(user.Id, actualUser.Id);
        Assert.Equal(user.Login, actualUser.Login);
        Assert.Equal(user.Role, actualUser.Role);
    }

    [Fact]
    public async Task GetUserById_WrongId()
    {
        //Arrange
        await _dbFixture.ResetDatabaseAsync();
        await using var context = _dbFixture.CreateContext();
        var repository = new UsersRepository(context);

        var user = CreateUser(context, "User");;

        //Act
        var actualUser = await repository.GetUserByIdAsync(Guid.NewGuid());

        //Assert
        Assert.Null(actualUser);
    }

    [Fact]
    public async Task GetUserById_WrongLogin()
    {
        //Arrange
        await _dbFixture.ResetDatabaseAsync();
        await using var context = _dbFixture.CreateContext();
        var repository = new UsersRepository(context);

        var user = CreateUser(context, "User");

        //Act
        var actualUser = await repository.GetUserByLoginAsync("User1");

        //Assert
        Assert.Null(actualUser);
    }

    [Fact]
    public async Task AddUser_DuplicateLogin()
    {
       //Arrange
        await _dbFixture.ResetDatabaseAsync();
        await using var context = _dbFixture.CreateContext();
        var repository = new UsersRepository(context);

        await repository.AddUserAsync(new User(Guid.NewGuid(), "User", "Password", UserRole.User));
        await repository.SaveChangesAsync();

        //Assert
        await repository.AddUserAsync(new User(Guid.NewGuid(), "User", "Password", UserRole.User));
        var error = await Assert.ThrowsAsync<DbUpdateException>(() => repository.SaveChangesAsync());
    }

    private Guid CreateUser(AppDbContext context, string login = "User")
    {
        var userId = Guid.NewGuid();
        context.Users.Add(new UserEntity()
        {
            Id = userId,
            Login = login,
            PasswordHash = "Password",
            Role = "User",
        });

        return userId;
    }
}
