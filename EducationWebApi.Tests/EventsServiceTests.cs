using EducationWebApi.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EducationWebApi.Tests;

public class EventsServiceTests
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IServiceScope _scope;
    private readonly IEventsService _eventsService;

    public EventsServiceTests()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));
        services.AddScoped<IEventsService, EventsService>();

        _serviceProvider = services.BuildServiceProvider();
        _scope = _serviceProvider.CreateScope();
        _eventsService = _scope.ServiceProvider.GetRequiredService<IEventsService>();
    }

    [Fact]
    public async Task CreateEvent_Ok()
    {
        //Arrange
        var newEvent = new CreateEventRequestDto() { Title = "event1", StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 2), TotalSeats = 1 };

        //Act
        var actual = await _eventsService.AddEventAsync(newEvent);

        //Assert
        Assert.NotNull(actual);
        Assert.Equal(newEvent.Title, actual.Title);
        Assert.Equal(newEvent.StartAt, actual.StartAt);
        Assert.Equal(newEvent.EndAt, actual.EndAt);
        Assert.Equal(newEvent.TotalSeats, actual.TotalSeats);
        Assert.Equal(newEvent.TotalSeats, actual.AvailableSeats);
    }

    [Fact]
    public async Task GetAllEvents_Ok()
    {
        //Arrange
        var events = new[]
        {
            new CreateEventRequestDto() { Title = "event1", StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 2), TotalSeats = 1 },
            new CreateEventRequestDto() { Title = "event2", StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 2), TotalSeats = 1 },
            new CreateEventRequestDto() { Title = "event3", StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 2), TotalSeats = 1 }
        };
        foreach (var item in events)
        {
            await _eventsService.AddEventAsync(item);
        }

        //Act
        var actual = await _eventsService.GetEventsAsync(new EventFilterDto(), new PagingRequestDto());

        //Assert
        Assert.Equal(events.Length, actual.Data.Length);
        Assert.Equal(events.Length, actual.TotalCount);
        Assert.Equal(events.Select(item => item.Title), actual.Data.Select(item => item.Title).Order());
    }

    [Fact]
    public async Task GetEvent_Ok()
    {
        //Arrange
        var id = await CreateEvent("event1", startAt: new DateTime(2026, 1, 1), endAt: new DateTime(2026, 1, 2));

        //Act
        var actual = await _eventsService.GetEventAsync(id);

        //Assert
        Assert.NotNull(actual);
        Assert.Equal(id, actual.Id);
        Assert.Equal("event1", actual.Title);
        Assert.Equal(new DateTime(2026, 1, 1), actual.StartAt);
        Assert.Equal(new DateTime(2026, 1, 2), actual.EndAt);
    }

    [Fact]
    public async Task UpdateEvent_Ok()
    {
        //Arrange
        var id = await CreateEvent("event1", startAt: new DateTime(2026, 1, 1), endAt: new DateTime(2026, 1, 2));
        var newEvent = new UpdateEventRequestDto() { Title = "event2", StartAt = new DateTime(2026, 1, 2), EndAt = new DateTime(2026, 1, 3) };

        //Act
        var actual = await _eventsService.ChangeEventAsync(id, newEvent);

        //Assert
        Assert.NotNull(actual);
        Assert.Equal(id, actual.Id);
        Assert.Equal(newEvent.Title, actual.Title);
        Assert.Equal(newEvent.StartAt, actual.StartAt);
        Assert.Equal(newEvent.EndAt, actual.EndAt);
    }

    [Fact]
    public async Task RemoveEvent_Ok()
    {
        //Arrange
        var id = await CreateEvent("event1");

        //Act
        var removeResult = await _eventsService.RemoveEventAsync(id);

        //Assert
        Assert.True(removeResult);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _eventsService.GetEventAsync(id));
    }

    [Fact]
    public async Task GetAllEvents_FilterByTitle_Ok()
    {
        //Arrange
        await CreateEvent("event1", startAt: new DateTime(2026, 1, 1), endAt: new DateTime(2026, 1, 2));
        await CreateEvent("event2", startAt: new DateTime(2026, 1, 1), endAt: new DateTime(2026, 1, 2));
        await CreateEvent("event3", startAt: new DateTime(2026, 1, 1), endAt: new DateTime(2026, 1, 2));

        //Act
        var actual = await _eventsService.GetEventsAsync(new EventFilterDto() { Title = "event2" }, new PagingRequestDto());

        //Assert
        Assert.Single(actual.Data);
        Assert.Equal(["event2"], actual.Data.Select(item => item.Title));
    }

    [Fact]
    public async Task GetAllEvents_FilterByStartAt_Ok()
    {
        //Arrange
        await CreateEvent("event1", startAt: new DateTime(2026, 1, 1), endAt: new DateTime(2026, 1, 2));
        await CreateEvent("event2", startAt: new DateTime(2026, 1, 2), endAt: new DateTime(2026, 1, 3));
        await CreateEvent("event3", startAt: new DateTime(2026, 1, 3), endAt: new DateTime(2026, 1, 4));

        //Act
        var actual = await _eventsService.GetEventsAsync(new EventFilterDto() { From = new DateTime(2026, 1, 2) }, new PagingRequestDto());

        //Assert
        Assert.Equal(2, actual.Data.Length);
        Assert.Equal(["event2", "event3"], actual.Data.Select(item => item.Title).Order());
    }

    [Fact]
    public async Task GetAllEvents_FilterByEndAt_Ok()
    {
        //Arrange
        await CreateEvent("event1", startAt: new DateTime(2026, 1, 1), endAt: new DateTime(2026, 1, 2));
        await CreateEvent("event2", startAt: new DateTime(2026, 1, 2), endAt: new DateTime(2026, 1, 3));
        await CreateEvent("event3", startAt: new DateTime(2026, 1, 3), endAt: new DateTime(2026, 1, 4));

        //Act
        var actual = await _eventsService.GetEventsAsync(new EventFilterDto() { To = new DateTime(2026, 1, 3) }, new PagingRequestDto());

        //Assert
        Assert.Equal(2, actual.Data.Length);
        Assert.Equal(["event1", "event2"], actual.Data.Select(item => item.Title).Order());
    }

    [Fact]
    public async Task GetAllEvents_FilterMultiple_Ok()
    {
        //Arrange
        await CreateEvent("event1", startAt: new DateTime(2026, 1, 1), endAt: new DateTime(2026, 1, 2));
        await CreateEvent("event2", startAt: new DateTime(2026, 1, 2), endAt: new DateTime(2026, 1, 3));
        await CreateEvent("event2", startAt: new DateTime(2026, 1, 3), endAt: new DateTime(2026, 1, 4));
        await CreateEvent("event2", startAt: new DateTime(2026, 1, 3), endAt: new DateTime(2026, 1, 5));
        await CreateEvent("event3", startAt: new DateTime(2026, 1, 5), endAt: new DateTime(2026, 1, 6));

        //Act
        var actualResult = await _eventsService.GetEventsAsync(
            new EventFilterDto() { Title = "event2", From = new DateTime(2026, 1, 3), To = new DateTime(2026, 1, 4) }, 
            new PagingRequestDto()
        );

        //Assert
        Assert.Single(actualResult.Data);
        var actualItem = actualResult.Data.First();
        Assert.Equal("event2", actualItem.Title);
        Assert.Equal(new DateTime(2026, 1, 3), actualItem.StartAt);
        Assert.Equal(new DateTime(2026, 1, 4), actualItem.EndAt);
    }

    [Fact]
    public async Task GetAllEvents_Paging_Ok()
    {
        //Arrange
        await CreateEvent("event1", startAt: new DateTime(2026, 1, 1), endAt: new DateTime(2026, 1, 2));
        await CreateEvent("event2", startAt: new DateTime(2026, 1, 2), endAt: new DateTime(2026, 1, 3));
        await CreateEvent("event3", startAt: new DateTime(2026, 1, 3), endAt: new DateTime(2026, 1, 4));

        string[] expectedPage1Titles = ["event1", "event2"];
        string[] expectedPage2Titles = ["event3"];

        //Act
        var actualPage1 = await _eventsService.GetEventsAsync(new EventFilterDto(), new PagingRequestDto() { Page = 1, PageSize = 2 });
        var actualPage2 = await _eventsService.GetEventsAsync(new EventFilterDto(), new PagingRequestDto() { Page = 2, PageSize = 2 });

        //Assert
        Assert.Equal(actualPage1.Data.Select(x => x.Title), expectedPage1Titles);
        Assert.Equal(3, actualPage1.TotalCount);
        Assert.Equal(2, actualPage1.PageSize);
        Assert.Equal(1, actualPage1.CurrentPage);

        Assert.Equal(actualPage2.Data.Select(x => x.Title), expectedPage2Titles);
        Assert.Equal(3, actualPage2.TotalCount);
        Assert.Equal(1, actualPage2.PageSize);
        Assert.Equal(2, actualPage2.CurrentPage);
    }

    [Fact]
    public async Task GetEvent_WrongId()
    {
        //Arrange
        await CreateEvent("event1");
        var id = new Guid();

        //Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _eventsService.GetEventAsync(id));
        Assert.Equal($"Event Id: {id} not found", exception.Message);
    }

    [Fact]
    public async Task UpdateEvent_WrongId()
    {
        //Arrange
        await CreateEvent("event1", startAt: new DateTime(2026, 1, 1), endAt: new DateTime(2026, 1, 2));
        var id = new Guid();

        //Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _eventsService.ChangeEventAsync(
                id, 
                new UpdateEventRequestDto() { Title = "event2", StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 2) }
            ));
        Assert.Equal($"Event Id: {id} not found", exception.Message);
    }

    private async Task<Guid> CreateEvent(string title, DateTime? startAt = null, DateTime? endAt = null, int totalSeats = 1)
    {
        var eventSource = new CreateEventRequestDto()
        { 
            Title = title, 
            StartAt = startAt ?? new DateTime(2026, 1, 1), 
            EndAt = endAt ?? new DateTime(2026, 1, 2), 
            TotalSeats = totalSeats 
        };
        var result = await _eventsService.AddEventAsync(eventSource);

        return result.Id;
    }
}
