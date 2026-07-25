using EducationWebApi.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;
using EducationWebApi.Domain;

namespace EducationWebApi.Tests;

[Collection("RepositoryTestCollection")]
public class EventsRepositoryTests
{
    readonly RepositoryTestFixture _dbFixture;

    public EventsRepositoryTests(RepositoryTestFixture dbFixture)
    {
        _dbFixture = dbFixture;
    }

    [Fact]
    public async Task AddEvent_Ok()
    {
        //Arrange
        await _dbFixture.ResetDatabaseAsync();
        await using var context = _dbFixture.CreateContext();
        var repository = new EventsRepository(context);

        var expectedEvent = new Event("Event", "Fantastic Event", DateTime.UtcNow, DateTime.UtcNow, 10);

        //Act
        await repository.AddEventAsync(expectedEvent);
        await repository.SaveChangesAsync();

        //Assert
        await using var verifyContext = _dbFixture.CreateContext();
        var actualEvent = await verifyContext.Events.FirstOrDefaultAsync(b => b.Id == expectedEvent.Id);
        Assert.NotNull(actualEvent);
        Assert.Equal(expectedEvent.Id, actualEvent.Id);
        Assert.Equal(expectedEvent.Title, actualEvent.Title);
        Assert.Equal(expectedEvent.Description, actualEvent.Description);
        Assert.Equal(expectedEvent.StartAt, actualEvent.StartAt);
        Assert.Equal(expectedEvent.EndAt, actualEvent.EndAt);
        Assert.Equal(expectedEvent.TotalSeats, actualEvent.TotalSeats);
        Assert.Equal(expectedEvent.TotalSeats, actualEvent.AvailableSeats);
    }

    [Fact]
    public async Task GetEventById_Ok()
    {
        //Arrange
        await _dbFixture.ResetDatabaseAsync();
        await using var context = _dbFixture.CreateContext();
        var repository = new EventsRepository(context);

        var eventId1 = CreateEvent(context, "Event1");
        var eventId2 = CreateEvent(context, "Event2");
        await repository.SaveChangesAsync();

        //Act
        var actualEvent = await repository.GetEventByIdAsync(eventId1);

        //Assert
        Assert.NotNull(actualEvent);
        Assert.Equal(eventId1, actualEvent.Id);
        Assert.Equal("Event1", actualEvent.Title);
    }

    [Fact]
    public async Task GetEventById_WrongId()
    {
        //Arrange
        await _dbFixture.ResetDatabaseAsync();
        await using var context = _dbFixture.CreateContext();
        var repository = new EventsRepository(context);

        var eventId1 = CreateEvent(context, "Event1");
        await repository.SaveChangesAsync();

        //Act
        var actualEvent = await repository.GetEventByIdAsync(Guid.NewGuid());

        //Assert
        Assert.Null(actualEvent);
    }

    [Fact]
    public async Task GetAllEvents_Ok()
    {
        //Arrange
        await _dbFixture.ResetDatabaseAsync();
        await using var context = _dbFixture.CreateContext();
        var repository = new EventsRepository(context);

        var eventId1 = CreateEvent(context, "Event1");
        var eventId2 = CreateEvent(context, "Event2");
        var eventId3 = CreateEvent(context, "Event3");
        await repository.SaveChangesAsync();

        //Act
        var events = await repository.GetAllEventsAsync(new EventFilter(), new PagingRequest(1, 10));

        //Assert
        Assert.Equal(3, events.TotalCount);
        Assert.Equal(1, events.CurrentPage);
        Assert.Equal(3, events.PageSize);
        Assert.Equal(["Event1", "Event2", "Event3"], events.Data.Select(item => item.Title));
    }

    [Fact]
    public async Task GetAllEvents_Paging_Ok()
    {
        //Arrange
        await _dbFixture.ResetDatabaseAsync();
        await using var context = _dbFixture.CreateContext();
        var repository = new EventsRepository(context);

        var eventId1 = CreateEvent(context, "Event1");
        var eventId2 = CreateEvent(context, "Event2");
        var eventId3 = CreateEvent(context, "Event3");
        await repository.SaveChangesAsync();

        //Act
        var page1 = await repository.GetAllEventsAsync(new EventFilter(), new PagingRequest(1, 2));
        var page2 = await repository.GetAllEventsAsync(new EventFilter(), new PagingRequest(2, 2));

        //Assert
        Assert.Equal(3, page1.TotalCount);
        Assert.Equal(1, page1.CurrentPage);
        Assert.Equal(2, page1.PageSize);
        Assert.Equal(["Event1", "Event2"], page1.Data.Select(item => item.Title));

        Assert.Equal(3, page2.TotalCount);
        Assert.Equal(2, page2.CurrentPage);
        Assert.Equal(1, page2.PageSize);
        Assert.Equal(["Event3"], page2.Data.Select(item => item.Title));
    }

    [Fact]
    public async Task GetAllEvents_FilterByTitle_Ok()
    {
        //Arrange
        await _dbFixture.ResetDatabaseAsync();
        await using var context = _dbFixture.CreateContext();
        var repository = new EventsRepository(context);

        var eventId1 = CreateEvent(context, "Event1");
        var eventId2 = CreateEvent(context, "Event2");
        var eventId3 = CreateEvent(context, "Event3");
        await repository.SaveChangesAsync();

        //Act
        var page = await repository.GetAllEventsAsync(new EventFilter() {Title = "Event2"}, new PagingRequest(1, 10));

        //Assert
        Assert.Equal(["Event2"], page.Data.Select(item => item.Title).ToArray());
    }

    [Fact]
    public async Task GetAllEvents_FilterByStartAt_Ok()
    {
        //Arrange
        await _dbFixture.ResetDatabaseAsync();
        await using var context = _dbFixture.CreateContext();
        var repository = new EventsRepository(context);
        var filterFrom = new DateTime(2026, 1, 2).ToUniversalTime();

        var eventId1 = CreateEvent(context, "Event1", startAt: new DateTime(2026, 1, 1).ToUniversalTime());
        var eventId2 = CreateEvent(context, "Event2", startAt: filterFrom);
        var eventId3 = CreateEvent(context, "Event3", startAt: new DateTime(2026, 1, 3).ToUniversalTime());
        await repository.SaveChangesAsync();

        //Act
        var page = await repository.GetAllEventsAsync(new EventFilter() {From = filterFrom}, new PagingRequest(1, 10));

        //Assert
        Assert.Equal(["Event2", "Event3"], page.Data.Select(item => item.Title).ToArray());
    }

    [Fact]
    public async Task GetAllEvents_FilterByEndAt_Ok()
    {
        //Arrange
        await _dbFixture.ResetDatabaseAsync();
        await using var context = _dbFixture.CreateContext();
        var repository = new EventsRepository(context);
        var filterTo = new DateTime(2026, 1, 2).ToUniversalTime();

        var eventId1 = CreateEvent(context, "Event1", endAt: new DateTime(2026, 1, 1).ToUniversalTime());
        var eventId2 = CreateEvent(context, "Event2", endAt: filterTo);
        var eventId3 = CreateEvent(context, "Event3", endAt: new DateTime(2026, 1, 3).ToUniversalTime());
        await repository.SaveChangesAsync();

        //Act
        var page = await repository.GetAllEventsAsync(new EventFilter() {To = filterTo}, new PagingRequest(1, 10));

        //Assert
        Assert.Equal(["Event1", "Event2"], page.Data.Select(item => item.Title).ToArray());
    }

    [Fact]
    public async Task GetAllEvents_MultipleFilter_Ok()
    {
        //Arrange
        await _dbFixture.ResetDatabaseAsync();
        await using var context = _dbFixture.CreateContext();
        var repository = new EventsRepository(context);
        var filterFrom = new DateTime(2026, 1, 1).ToUniversalTime();
        var filterTo = new DateTime(2026, 1, 2).ToUniversalTime();

        var eventId1 = CreateEvent(context, "Event1", startAt: filterFrom, endAt: new DateTime(2026, 1, 1).ToUniversalTime());
        var eventId2 = CreateEvent(context, "Event2", startAt: filterFrom, endAt: filterTo);
        var eventId3 = CreateEvent(context, "Event3", startAt: filterFrom, endAt: new DateTime(2026, 1, 3).ToUniversalTime());
        await repository.SaveChangesAsync();

        //Act
        var page = await repository.GetAllEventsAsync(new EventFilter() {Title = "Event2", From = filterFrom, To = filterTo}, new PagingRequest(1, 10));

        //Assert
        Assert.Equal(["Event2"], page.Data.Select(item => item.Title).ToArray());
    }

    [Fact]
    public async Task ChangeEvent_Ok()
    {
        //Arrange
        await _dbFixture.ResetDatabaseAsync();
        await using var context = _dbFixture.CreateContext();
        var repository = new EventsRepository(context);

        var eventId = CreateEvent(context, 
            title: "Event", 
            startAt: new DateTime(2025, 1, 1).ToUniversalTime(),
            endAt: new DateTime(2025, 1, 2).ToUniversalTime(), 
            totalSeats: 5
        );
        await repository.SaveChangesAsync();

        //Act
        var eventToChange = new Event(
            id: eventId,
            title: "Title",
            description: "Description",
            startAt: new DateTime(2026, 1, 1).ToUniversalTime(),
            endAt: new DateTime(2026, 1, 2).ToUniversalTime(),
            totalSeats: 10
        );
        await repository.ChangeEventAsync(eventToChange);
        await repository.SaveChangesAsync();

        //Assert
        await using var verifyContext = _dbFixture.CreateContext();
        var actualEvent = await verifyContext.Events.FirstOrDefaultAsync(b => b.Id == eventId);
        Assert.NotNull(actualEvent);
        Assert.Equal(eventToChange.Title, actualEvent.Title);
        Assert.Equal(eventToChange.Description, actualEvent.Description);
        Assert.Equal(eventToChange.StartAt, actualEvent.StartAt);
        Assert.Equal(eventToChange.EndAt, actualEvent.EndAt);
        Assert.Equal(eventToChange.TotalSeats, actualEvent.TotalSeats);
    }

    [Fact]
    public async Task ChangeEvent_WrongId()
    {
        //Arrange
        await _dbFixture.ResetDatabaseAsync();
        await using var context = _dbFixture.CreateContext();
        var repository = new EventsRepository(context);

        var eventId = CreateEvent(context, title: "Event");
        await repository.SaveChangesAsync();

        //Assert
        var eventToChange = new Event(
            id: Guid.NewGuid(),
            title: "Title",
            description: "Description",
            startAt: new DateTime(2026, 1, 1).ToUniversalTime(),
            endAt: new DateTime(2026, 1, 2).ToUniversalTime(),
            totalSeats: 10
        );
        var error = await Assert.ThrowsAsync<KeyNotFoundException>(() => repository.ChangeEventAsync(eventToChange));;
    }

    [Fact]
    public async Task RemoveEvent_Ok()
    {
        //Arrange
        await _dbFixture.ResetDatabaseAsync();
        await using var context = _dbFixture.CreateContext();
        var repository = new EventsRepository(context);

        var eventId = CreateEvent(context, "Event1");
        await repository.SaveChangesAsync();

        //Act
        await repository.RemoveEventAsync(eventId);
        await repository.SaveChangesAsync();

        //Assert
        await using var verifyContext = _dbFixture.CreateContext();
        var actualEvent = await verifyContext.Events.FirstOrDefaultAsync(b => b.Id == eventId);
        Assert.Null(actualEvent);
    }

    [Fact]
    public async Task RemoveEvent_WrongId()
    {
        //Arrange
        await _dbFixture.ResetDatabaseAsync();
        await using var context = _dbFixture.CreateContext();
        var repository = new EventsRepository(context);

        //Assert
        var error = await Assert.ThrowsAsync<KeyNotFoundException>(() => repository.RemoveEventAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task LoadEventsWithBookings_ReturnsCorrectCount()
    {
        // Arrange
        await _dbFixture.ResetDatabaseAsync();
        await using var context = _dbFixture.CreateContext();
        var eventId1 = CreateEvent(context, "Event1");
        var eventId2 = CreateEvent(context, "Event2");
        var expectedBookingId1 = Guid.NewGuid();
        var expectedBookingId2 = Guid.NewGuid();
        var userId = Guid.NewGuid();

        context.Bookings.AddRange(
            new BookingEntity { Id = expectedBookingId1, UserId = userId, EventId = eventId1, Status = "Pending", CreatedAt = DateTime.UtcNow },
            new BookingEntity { Id = expectedBookingId2, UserId = userId, EventId = eventId1, Status = "Pending", CreatedAt = DateTime.UtcNow },
            new BookingEntity { Id = Guid.NewGuid(), UserId = userId, EventId = eventId2, Status = "Pending", CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        // Act
        await using var verifyContext = _dbFixture.CreateContext();
        var loaded = await verifyContext.Events
            .Include(e => e.Bookings)
            .FirstAsync(e => e.Id == eventId1);

        // Assert
        Assert.NotNull(loaded.Bookings);
        Assert.Equal(2, loaded.Bookings.Count);
        var bookingsIds = loaded.Bookings.Select(item => item.Id);
        Assert.Contains(expectedBookingId1, bookingsIds);
        Assert.Contains(expectedBookingId2, bookingsIds);
    } 

    private Guid CreateEvent(
        AppDbContext context, 
        string title = "Event", 
        DateTime? startAt = null,
        DateTime? endAt = null,
        int totalSeats = 10
    )
    {
        var eventId = Guid.NewGuid();
        context.Events.Add(new EventEntity()
        {
            Id = eventId,
            Title = title,
            StartAt = startAt ?? DateTime.UtcNow,
            EndAt = endAt ?? DateTime.UtcNow,
            TotalSeats = totalSeats,
            AvailableSeats = totalSeats,
        });

        return eventId;
    }
}
