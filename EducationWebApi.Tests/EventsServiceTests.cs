namespace EducationWebApi.Tests;

public class EventsServiceTests
{
    private readonly IEventsService _eventsService;

    public EventsServiceTests()
    {
        _eventsService = new EventsService();
    }

    [Fact]
    public void CreateEvent_Ok()
    {
        EventsServiceClear();

        //Arrange
        var newEvent = new EventRequestDto() { Title = "event1", StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 2) };

        //Act
        var actual = _eventsService.AddEvent(newEvent);

        //Assert
        Assert.NotNull(actual);
        Assert.Equal(newEvent.Title, actual.Title);
        Assert.Equal(newEvent.StartAt, actual.StartAt);
        Assert.Equal(newEvent.EndAt, actual.EndAt);
    }

    [Fact]
    public void GetAllEvents_Ok()
    {
        EventsServiceClear();

        //Arrange
        var events = new[]
        {
            new EventRequestDto() { Title = "event1", StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 2) },
            new EventRequestDto() { Title = "event2", StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 2) },
            new EventRequestDto() { Title = "event3", StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 2) }
        };
        foreach (var item in events)
        {
            _eventsService.AddEvent(item);
        }

        //Act
        var actual = _eventsService.GetEvents(new EventFilterDto(), new PagingRequestDto());

        //Assert
        Assert.Equal(events.Length, actual.Data.Length);
        Assert.Equal(events.Length, actual.TotalCount);
        Assert.Equal(events.Select(item => item.Title), actual.Data.Select(item => item.Title).Order());
    }

    [Fact]
    public void GetEvent_Ok()
    {
        EventsServiceClear();

        //Arrange
        var newEvent = new EventRequestDto() { Title = "event1", StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 2) };
        var id = _eventsService.AddEvent(newEvent).Id;

        //Act
        var actual = _eventsService.GetEvent(id);

        //Assert
        Assert.NotNull(actual);
        Assert.Equal(id, actual.Id);
        Assert.Equal(newEvent.Title, actual.Title);
        Assert.Equal(newEvent.StartAt, actual.StartAt);
        Assert.Equal(newEvent.EndAt, actual.EndAt);
    }

    [Fact]
    public void UpdateEvent_Ok()
    {
        EventsServiceClear();

        //Arrange
        var oldEvent = new EventRequestDto() { Title = "event1", StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 2) };
        var id = _eventsService.AddEvent(oldEvent).Id;
        var newEvent = new EventRequestDto() { Title = "event2", StartAt = new DateTime(2026, 1, 2), EndAt = new DateTime(2026, 1, 3) };

        //Act
        var actual = _eventsService.ChangeEvent(id, newEvent);

        //Assert
        Assert.NotNull(actual);
        Assert.Equal(id, actual.Id);
        Assert.Equal(newEvent.Title, actual.Title);
        Assert.Equal(newEvent.StartAt, actual.StartAt);
        Assert.Equal(newEvent.EndAt, actual.EndAt);
    }

    [Fact]
    public void RemoveEvent_Ok()
    {
        EventsServiceClear();

        //Arrange
        var newEvent = new EventRequestDto() { Title = "event1", StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 2) };
        var id = _eventsService.AddEvent(newEvent).Id;

        //Act
        var removeResult = _eventsService.RemoveEvent(id);

        //Assert
        Assert.True(removeResult);
        Assert.Throws<KeyNotFoundException>(() => _eventsService.GetEvent(id));
    }

    [Fact]
    public void GetAllEvents_FilterByTitle_Ok()
    {
        EventsServiceClear();

        //Arrange
        var events = new[]
        {
            new EventRequestDto() { Title = "event1", StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 2) },
            new EventRequestDto() { Title = "event2", StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 2) },
            new EventRequestDto() { Title = "event3", StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 2) }
        };
        foreach (var item in events)
        {
            _eventsService.AddEvent(item);
        }

        //Act
        var actual = _eventsService.GetEvents(new EventFilterDto() { Title = "event2" }, new PagingRequestDto());

        //Assert
        Assert.Single(actual.Data);
        Assert.Equal(["event2"], actual.Data.Select(item => item.Title));
    }

    [Fact]
    public void GetAllEvents_FilterByStartAt_Ok()
    {
        EventsServiceClear();

        //Arrange
        var events = new[]
        {
            new EventRequestDto() { Title = "event1", StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 2) },
            new EventRequestDto() { Title = "event2", StartAt = new DateTime(2026, 1, 2), EndAt = new DateTime(2026, 1, 3) },
            new EventRequestDto() { Title = "event3", StartAt = new DateTime(2026, 1, 3), EndAt = new DateTime(2026, 1, 4) }
        };
        foreach (var item in events)
        {
            _eventsService.AddEvent(item);
        }

        //Act
        var actual = _eventsService.GetEvents(new EventFilterDto() { From = new DateTime(2026, 1, 2) }, new PagingRequestDto());

        //Assert
        Assert.Equal(2, actual.Data.Length);
        Assert.Equal(["event2", "event3"], actual.Data.Select(item => item.Title).Order());
    }

    [Fact]
    public void GetAllEvents_FilterByEndAt_Ok()
    {
        EventsServiceClear();

        //Arrange
        var events = new[]
        {
            new EventRequestDto() { Title = "event1", StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 2) },
            new EventRequestDto() { Title = "event2", StartAt = new DateTime(2026, 1, 2), EndAt = new DateTime(2026, 1, 3) },
            new EventRequestDto() { Title = "event3", StartAt = new DateTime(2026, 1, 3), EndAt = new DateTime(2026, 1, 4) }
        };
        foreach (var item in events)
        {
            _eventsService.AddEvent(item);
        }

        //Act
        var actual = _eventsService.GetEvents(new EventFilterDto() { To = new DateTime(2026, 1, 3) }, new PagingRequestDto());

        //Assert
        Assert.Equal(2, actual.Data.Length);
        Assert.Equal(["event1", "event2"], actual.Data.Select(item => item.Title).Order());
    }

    [Fact]
    public void GetAllEvents_FilterMultiple_Ok()
    {
        EventsServiceClear();

        //Arrange
        var events = new[]
        {
            new EventRequestDto() { Title = "event1", StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 2) },
            new EventRequestDto() { Title = "event2", StartAt = new DateTime(2026, 1, 2), EndAt = new DateTime(2026, 1, 3) },
            new EventRequestDto() { Title = "event2", StartAt = new DateTime(2026, 1, 3), EndAt = new DateTime(2026, 1, 4) },
            new EventRequestDto() { Title = "event2", StartAt = new DateTime(2026, 1, 3), EndAt = new DateTime(2026, 1, 5) },
            new EventRequestDto() { Title = "event3", StartAt = new DateTime(2026, 1, 5), EndAt = new DateTime(2026, 1, 6) }
        };
        foreach (var item in events)
        {
            _eventsService.AddEvent(item);
        }

        //Act
        var actualResult = _eventsService.GetEvents(
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
    public void GetAllEvents_Paging_Ok()
    {
        EventsServiceClear();

        //Arrange
        var events = new[]
        {
            new EventRequestDto() { Title = "event1", StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 2) },
            new EventRequestDto() { Title = "event2", StartAt = new DateTime(2026, 1, 2), EndAt = new DateTime(2026, 1, 3) },
            new EventRequestDto() { Title = "event3", StartAt = new DateTime(2026, 1, 3), EndAt = new DateTime(2026, 1, 4) }
        };
        foreach (var item in events)
        {
            _eventsService.AddEvent(item);
        }

        string[] expectedPage1Titles = ["event1", "event2"];
        string[] expectedPage2Titles = ["event3"];

        //Act
        var actualPage1 = _eventsService.GetEvents(new EventFilterDto(), new PagingRequestDto() { Page = 1, PageSize = 2 });
        var actualPage2 = _eventsService.GetEvents(new EventFilterDto(), new PagingRequestDto() { Page = 2, PageSize = 2 });

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
    public void GetEvent_WrongId()
    {
        EventsServiceClear();

        //Arrange
        _eventsService.AddEvent(new EventRequestDto() { Title = "event1", StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 2) });
        var id = new Guid();

        //Assert
        var exception = Assert.Throws<KeyNotFoundException>(() => _eventsService.GetEvent(id));
        Assert.Equal($"Event Id: {id} not found", exception.Message);
    }

    [Fact]
    public void UpdateEvent_WrongId()
    {
        EventsServiceClear();

        //Arrange
        _eventsService.AddEvent(new EventRequestDto() { Title = "event1", StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 2) });
        var id = new Guid();

        //Assert
        var exception = Assert.Throws<KeyNotFoundException>(() => _eventsService.ChangeEvent(id, new EventRequestDto() { Title = "event2", StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 2) }));
        Assert.Equal($"Event Id: {id} not found", exception.Message);
    }

    private void EventsServiceClear()
    {
        var events = _eventsService.GetEvents(new EventFilterDto(), new PagingRequestDto());
        foreach (var item in events.Data)
        {
            _eventsService.RemoveEvent(item.Id);
        }
    }
}
