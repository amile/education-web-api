using Events.Application;
using Events.Domain;
using Microsoft.Extensions.Options;
using Moq;

namespace Events.Tests;

public class EventsCacheServiceTests
{
    private readonly Mock<IEventsRepository> _eventsRepository;
    private readonly Mock<ICacheService> _cache;
    private readonly IEventsService _eventsService;

    public EventsCacheServiceTests()
    {
        _eventsRepository = new Mock<IEventsRepository>();
        _cache = new Mock<ICacheService>();

        var options = Options.Create(new EventsCacheConfig());

        _eventsService = new EventsService(options, _eventsRepository.Object, _cache.Object);
    }

    [Fact]
    public async Task GetTopEvents_ReturnFromCache()
    {
        // Arrange
        var event1 = CreateEvent("event1");
        var event2 = CreateEvent("event2");
        var cachedEvents = new[] { EventDto.FromDomain(event1), EventDto.FromDomain(event2) };

        _cache
            .Setup(c => c.GetAsync<EventDto[]>(EventsCacheConstants.TopEventsKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedEvents);

        // Act
        var result = await _eventsService.GetTopEventsAsync();

        // Assert
        Assert.Equal(2, result.Count());

        _eventsRepository.Verify(
            r => r.GetTopEventsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetTopEvents_ReturnFromDb()
    {
        // Arrange
        var event1 = CreateEvent("event1");
        var event2 = CreateEvent("event2");

        _cache
            .Setup(c => c.GetAsync<EventDto[]>(EventsCacheConstants.TopEventsKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventDto[]?)null);

        _eventsRepository
            .Setup(r => r.GetTopEventsAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync([event1, event2]);

        // Act
        var result = await _eventsService.GetTopEventsAsync();

        // Assert
        Assert.Equal(2, result.Count());

        _cache.Verify(
            c => c.SetAsync(
                EventsCacheConstants.TopEventsKey,
                It.IsAny<EventDto[]>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetEvent_ReturnFromCache()
    {
        // Arrange
        var _event = CreateEvent();
        var cachedEvent = EventDto.FromDomain(_event);

        _cache
            .Setup(c => c.GetAsync<EventDto>(EventsCacheConstants.EventKey(cachedEvent.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedEvent);

        // Act
        var result = await _eventsService.GetEventAsync(cachedEvent.Id);

        // Assert
        Assert.Equal(cachedEvent.Id, result.Id);

        _eventsRepository.Verify(
            r => r.GetEventByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetEvent_ReturnFromDb()
    {
        // Arrange
        var _event = CreateEvent();
        var eventId = _event.Id;

        _cache
            .Setup(c => c.GetAsync<EventDto>(EventsCacheConstants.EventKey(eventId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventDto?)null);

        _eventsRepository
            .Setup(r => r.GetEventByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_event);

        // Act
        var result = await _eventsService.GetEventAsync(eventId);

        // Assert
        Assert.Equal(eventId, result.Id);

        _cache.Verify(
            c => c.SetAsync(
                EventsCacheConstants.EventKey(eventId),
                It.IsAny<EventDto>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateEvent_InvalidateCache()
    {
        // Arrange
        var request = new CreateEventRequestDto
        {
            Title = "event",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(1),
            TotalSeats = 10
        };

        _eventsRepository
            .Setup(r => r.AddEventAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _eventsService.AddEventAsync(request);

        // Assert
        _cache.Verify(
            c => c.RemoveAsync(EventsCacheConstants.TopEventsKey, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ChangeEvent_InvalidateCache()
    {
        // Arrange
        var _event = CreateEvent();
        var eventId = _event.Id;

        _eventsRepository
            .Setup(r => r.GetEventByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_event);

        _eventsRepository
            .Setup(r => r.ChangeEventAsync(_event, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new UpdateEventRequestDto
        {
            Title = "changed event",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(1),
        };

        // Act
        await _eventsService.ChangeEventAsync(eventId, request);

        // Assert
        _cache.Verify(
            c => c.RemoveAsync(EventsCacheConstants.EventKey(eventId), It.IsAny<CancellationToken>()),
            Times.Once);

        _cache.Verify(
            c => c.RemoveAsync(EventsCacheConstants.TopEventsKey, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteEvent_InvalidateCache()
    {
        // Arrange
        var _event = CreateEvent();
        var eventId = _event.Id;

        _eventsRepository
            .Setup(r => r.GetEventByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_event);

        _eventsRepository
            .Setup(r => r.RemoveEventAsync(eventId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _eventsService.RemoveEventAsync(eventId);

        // Assert
        _cache.Verify(
            c => c.RemoveAsync(EventsCacheConstants.EventKey(eventId), It.IsAny<CancellationToken>()),
            Times.Once);

        _cache.Verify(
            c => c.RemoveAsync(EventsCacheConstants.TopEventsKey, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private Event CreateEvent(string title = "event", DateTime? startAt = null, DateTime? endAt = null, int totalSeats = 1)
    {
        var eventSource = new Event(
            Guid.NewGuid(),
            title,
            null,
            startAt ?? new DateTime(2026, 1, 1), 
            endAt ?? new DateTime(2026, 1, 2), 
            totalSeats
        );
        return eventSource;
    }
}
