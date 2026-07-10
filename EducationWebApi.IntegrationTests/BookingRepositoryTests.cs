using EducationWebApi.DAL;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EducationWebApi.Tests;

public class BookingRepositoryTests : RepositoryTestsBase
{

    public BookingRepositoryTests()
    {
    }

    [Fact]
    public async Task AddBooking_Ok()
    {
        //Arrange
        await ResetDatabaseAsync();
        await using var context = CreateContext();
        var repository = new BookingRepository(context);

        var eventId = CreateEvent(context);

        //Act
        var expectedBooking = await repository.AddBookingAsync(eventId);
        await repository.SaveChangesAsync();

        //Assert
        await using var verifyContext = CreateContext();
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
        await ResetDatabaseAsync();
        await using var context = CreateContext();
        var repository = new BookingRepository(context);

        var eventId = CreateEvent(context);
        var expectedBooking = await repository.AddBookingAsync(eventId);
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
        await ResetDatabaseAsync();
        await using var context = CreateContext();
        var repository = new BookingRepository(context);

        var eventId = CreateEvent(context);
        await repository.AddBookingAsync(eventId);
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
        await ResetDatabaseAsync();
        await using var context = CreateContext();
        var repository = new BookingRepository(context);

        var eventId = CreateEvent(context);
        var booking1 = await repository.AddBookingAsync(eventId);
        var booking2 = await repository.AddBookingAsync(eventId);
        var booking3 = await repository.AddBookingAsync(eventId);
        var booking4 = await repository.AddBookingAsync(eventId);
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
    public async Task ConfirmBooking_Ok()
    {
        //Arrange
        await ResetDatabaseAsync();
        await using var context = CreateContext();
        var repository = new BookingRepository(context);

        var eventId = CreateEvent(context);
        var pendingBooking = await repository.AddBookingAsync(eventId);
        await repository.SaveChangesAsync();

        //Act
        await repository.ConfirmBookingAsync(pendingBooking.Id);
        await repository.SaveChangesAsync();

        //Assert
        await using var verifyContext = CreateContext();
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
        await ResetDatabaseAsync();
        await using var context = CreateContext();
        var repository = new BookingRepository(context);

        var eventId = CreateEvent(context);
        var pendingBooking = await repository.AddBookingAsync(eventId);
        await repository.SaveChangesAsync();

        //Act
        await repository.RejectBookingAsync(pendingBooking.Id);
        await repository.SaveChangesAsync();

        //Assert
        await using var verifyContext = CreateContext();
        var rejectedBooking = await verifyContext.Bookings.FirstOrDefaultAsync(b => b.Id == pendingBooking.Id);
        Assert.NotNull(rejectedBooking);
        Assert.Equal(pendingBooking.Id, rejectedBooking.Id);
        Assert.Equal(BookingStatus.Pending, pendingBooking.Status);
        Assert.Equal(BookingStatus.Rejected.ToString(), rejectedBooking.Status);
        Assert.NotNull(rejectedBooking.ProcessedAt);
    }

    [Fact]
    public async Task BookingUpdateStatus_WrongId()
    {
        //Arrange
        await ResetDatabaseAsync();
        await using var context = CreateContext();
        var repository = new BookingRepository(context);

        var eventId = CreateEvent(context);
        var pendingBooking = await repository.AddBookingAsync(eventId);
        await repository.SaveChangesAsync();

        var error = await Assert.ThrowsAsync<KeyNotFoundException>(() => repository.UpdateStatusAsync(Guid.NewGuid(), BookingStatus.Confirmed));
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
}
