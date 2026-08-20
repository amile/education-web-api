using EducationWebApi.Domain;
using EducationWebApi.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EducationWebApi.Tests;

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

    [Fact]
    public async Task LoadUserWithBookings_ReturnsCorrectCount()
    {
        await _dbFixture.ResetDatabaseAsync();
        await using var context = _dbFixture.CreateContext();
        var eventId1 =  CreateEvent(context);
        var eventId2 =  CreateEvent(context);
        var userId1 = CreateUser(context, "User1");
        var userId2 = CreateUser(context, "User2");
        var expectedBookingId1 = Guid.NewGuid();
        var expectedBookingId2 = Guid.NewGuid();

        context.Bookings.AddRange(
            new BookingEntity { Id = expectedBookingId1, UserId = userId1, EventId = eventId1, Status = "Pending", CreatedAt = DateTime.UtcNow },
            new BookingEntity { Id = expectedBookingId2, UserId = userId1, EventId = eventId1, Status = "Pending", CreatedAt = DateTime.UtcNow },
            new BookingEntity { Id = Guid.NewGuid(), UserId = userId2, EventId = eventId2, Status = "Pending", CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        // Act
        await using var verifyContext = _dbFixture.CreateContext();
        var loaded = await verifyContext.Users
            .Include(e => e.Bookings)
            .FirstAsync(e => e.Id == userId1);

        // Assert
        Assert.NotNull(loaded.Bookings);
        Assert.Equal(2, loaded.Bookings.Count);
        var bookingsIds = loaded.Bookings.Select(item => item.Id);
        Assert.Contains(expectedBookingId1, bookingsIds);
        Assert.Contains(expectedBookingId2, bookingsIds);
    }

    private Guid CreateEvent(AppDbContext context)
    {
        var eventId = Guid.NewGuid();
        context.Events.Add(new EventEntity()
        {
            Id = eventId,
            Title = "Event",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow,
            TotalSeats = 1,
            AvailableSeats = 1,
        });

        return eventId;
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
