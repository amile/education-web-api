using EducationWebApi.Domain;
using EducationWebApi.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EducationWebApi.Tests;

[Collection("RepositoryTestCollection")]
public class BookingRepositoryTests
{
    readonly RepositoryTestFixture _dbFixture;

    public BookingRepositoryTests(RepositoryTestFixture dbFixture)
    {
        _dbFixture = dbFixture;
    }

    [Fact]
    public async Task AddBooking_Ok()
    {
        //Arrange
        await _dbFixture.ResetDatabaseAsync();
        await using var context = _dbFixture.CreateContext();
        var repository = new BookingRepository(context);

        var eventId = CreateEvent(context);
        var userId = CreateUser(context);

        //Act
        var expectedBooking = await repository.AddBookingAsync(eventId, userId);
        await repository.SaveChangesAsync();

        //Assert
        await using var verifyContext = _dbFixture.CreateContext();
        var actualBooking = await verifyContext.Bookings.FirstOrDefaultAsync(b => b.Id == expectedBooking.Id);
        Assert.NotNull(actualBooking);
        Assert.Equal(expectedBooking.Id, actualBooking.Id);
        Assert.Equal(expectedBooking.EventId, actualBooking.EventId);
        Assert.Equal(expectedBooking.Status.ToString(), actualBooking.Status);
        Assert.Equal(expectedBooking.CreatedAt, actualBooking.CreatedAt);
        Assert.Equal(expectedBooking.ProcessedAt, actualBooking.ProcessedAt);
    }

    [Fact]
    public async Task GetBookingById_Ok()
    {
        //Arrange
        await _dbFixture.ResetDatabaseAsync();
        await using var context = _dbFixture.CreateContext();
        var repository = new BookingRepository(context);

        var eventId = CreateEvent(context);
        var userId = CreateUser(context);
        var expectedBooking = await repository.AddBookingAsync(eventId, userId);
        await repository.SaveChangesAsync();

        //Act
        var actualBooking = await repository.GetBookingByIdAsync(expectedBooking.Id);

        //Assert
        Assert.NotNull(actualBooking);
        Assert.Equal(expectedBooking.Id, actualBooking.Id);
        Assert.Equal(expectedBooking.EventId, actualBooking.EventId);
        Assert.Equal(expectedBooking.Status, actualBooking.Status);
        Assert.Equal(expectedBooking.CreatedAt, actualBooking.CreatedAt);
        Assert.Equal(expectedBooking.ProcessedAt, actualBooking.ProcessedAt);
    }

    [Fact]
    public async Task GetBookingById_WrongId()
    {
        //Arrange
        await _dbFixture.ResetDatabaseAsync();
        await using var context = _dbFixture.CreateContext();
        var repository = new BookingRepository(context);

        var eventId = CreateEvent(context);
        var userId = CreateUser(context);
        await repository.AddBookingAsync(eventId, userId);
        await repository.SaveChangesAsync();

        //Act
        var booking = await repository.GetBookingByIdAsync(Guid.NewGuid());

        //Assert
        Assert.Null(booking);
    }

    [Fact]
    public async Task GetPendingBookings_Ok()
    {
        //Arrange
        await _dbFixture.ResetDatabaseAsync();
        await using var context = _dbFixture.CreateContext();
        var repository = new BookingRepository(context);

        var eventId = CreateEvent(context);
        var userId = CreateUser(context);
        var booking1 = await repository.AddBookingAsync(eventId, userId);
        var booking2 = await repository.AddBookingAsync(eventId, userId);
        var booking3 = await repository.AddBookingAsync(eventId, userId);
        var booking4 = await repository.AddBookingAsync(eventId, userId);
        await repository.SaveChangesAsync();

        await repository.ConfirmBookingAsync(booking1.Id);
        await repository.RejectBookingAsync(booking3.Id);
        await repository.SaveChangesAsync();

        //Act
        var pendingBookings = await repository.GetPendingBookingsAsync();

        //Assert
        Assert.Equal(2, pendingBookings.Count);
        var pendingBookingsIds = pendingBookings.Select(item => item.Id).ToArray();
        Assert.Contains(booking2.Id, pendingBookingsIds);
        Assert.Contains(booking4.Id, pendingBookingsIds);
    }

    [Fact]
    public async Task UpdateBookingStatus_Ok()
    {
        //Arrange
        await _dbFixture.ResetDatabaseAsync();
        await using var context = _dbFixture.CreateContext();
        var repository = new BookingRepository(context);

        var eventId = CreateEvent(context);
        var userId = CreateUser(context);
        var bookingId = Guid.NewGuid();
        await context.Bookings.AddAsync(new BookingEntity()
        {
            Id = bookingId,
            EventId = eventId,
            UserId = userId,
            Status = BookingStatus.Rejected.ToString(),
            CreatedAt = DateTime.UtcNow,
        });
        await repository.SaveChangesAsync();

        //Act
        await repository.UpdateStatusAsync(bookingId, BookingStatus.Confirmed);
        await repository.SaveChangesAsync();

        //Assert
        await using var verifyContext = _dbFixture.CreateContext();
        var confirmedBooking = await verifyContext.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
        Assert.NotNull(confirmedBooking);
        Assert.Equal(BookingStatus.Confirmed.ToString(), confirmedBooking.Status);
        Assert.NotNull(confirmedBooking.ProcessedAt);
    }

    [Fact]
    public async Task BookingUpdateStatus_WrongId()
    {
        //Arrange
        await _dbFixture.ResetDatabaseAsync();
        await using var context = _dbFixture.CreateContext();
        var repository = new BookingRepository(context);

        var eventId = CreateEvent(context);
        var userId = CreateUser(context);
        var pendingBooking = await repository.AddBookingAsync(eventId, userId);
        await repository.SaveChangesAsync();

        var error = await Assert.ThrowsAsync<KeyNotFoundException>(() => repository.UpdateStatusAsync(Guid.NewGuid(), BookingStatus.Confirmed));
    }

    [Fact]
    public async Task ConfirmBooking_Ok()
    {
        //Arrange
        await _dbFixture.ResetDatabaseAsync();
        await using var context = _dbFixture.CreateContext();
        var repository = new BookingRepository(context);

        var eventId = CreateEvent(context);
        var userId = CreateUser(context);
        var pendingBooking = await repository.AddBookingAsync(eventId, userId);
        await repository.SaveChangesAsync();

        //Act
        await repository.ConfirmBookingAsync(pendingBooking.Id);
        await repository.SaveChangesAsync();

        //Assert
        await using var verifyContext = _dbFixture.CreateContext();
        var confirmedBooking = await verifyContext.Bookings.FirstOrDefaultAsync(b => b.Id == pendingBooking.Id);
        Assert.NotNull(confirmedBooking);
        Assert.Equal(pendingBooking.Id, confirmedBooking.Id);
        Assert.Equal(BookingStatus.Pending, pendingBooking.Status);
        Assert.Equal(BookingStatus.Confirmed.ToString(), confirmedBooking.Status);
        Assert.NotNull(confirmedBooking.ProcessedAt);
    }

    [Fact]
    public async Task RejectBooking_Ok()
    {
        //Arrange
        await _dbFixture.ResetDatabaseAsync();
        await using var context = _dbFixture.CreateContext();
        var repository = new BookingRepository(context);

        var eventId = CreateEvent(context);
        var userId = CreateUser(context);
        var pendingBooking = await repository.AddBookingAsync(eventId, userId);
        await repository.SaveChangesAsync();

        //Act
        await repository.RejectBookingAsync(pendingBooking.Id);
        await repository.SaveChangesAsync();

        //Assert
        await using var verifyContext = _dbFixture.CreateContext();
        var rejectedBooking = await verifyContext.Bookings.FirstOrDefaultAsync(b => b.Id == pendingBooking.Id);
        Assert.NotNull(rejectedBooking);
        Assert.Equal(pendingBooking.Id, rejectedBooking.Id);
        Assert.Equal(BookingStatus.Pending, pendingBooking.Status);
        Assert.Equal(BookingStatus.Rejected.ToString(), rejectedBooking.Status);
        Assert.NotNull(rejectedBooking.ProcessedAt);
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
            TotalSeats = 10,
            AvailableSeats = 10,
        });

        return eventId;
    }

    private Guid CreateUser(AppDbContext context)
    {
        var userId = Guid.NewGuid();
        context.Users.Add(new UserEntity()
        {
            Id = userId,
            Login = "User",
            PasswordHash = "Password",
            Role = "User",
        });

        return userId;
    }
}
