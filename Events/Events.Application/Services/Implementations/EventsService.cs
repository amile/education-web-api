using System.ComponentModel.DataAnnotations;
using System.Data;
using Events.Domain;
using Microsoft.Extensions.Options;

namespace Events.Application;

public class EventsService : IEventsService
{
    private readonly IEventsRepository _eventsRepository;
    private readonly ICacheService _cache;
    private readonly EventsCacheConfig _config;

    public EventsService(IOptions<EventsCacheConfig> options, IEventsRepository eventsRepository, ICacheService cache)
    {
        _eventsRepository = eventsRepository;
        _cache = cache;
        _config = options.Value;
    }

    public async Task<PaginatedResultDto<EventDto>> GetEventsAsync(EventFilterDto filter, PagingRequestDto pagingRequest, CancellationToken ct = default)
    {
        var domainFilter = filter.ToDomain();
        var domainPaging = pagingRequest.ToDomain();
        var result = await _eventsRepository.GetAllEventsAsync(domainFilter, domainPaging, ct);
        var data = result.Data.Select(EventDto.FromDomain).ToArray();
    
        return new PaginatedResultDto<EventDto>(data, result.TotalCount, result.CurrentPage, result.PageSize);
    }

    public async Task<EventDto[]> GetTopEventsAsync(CancellationToken ct = default)
    {
        var cached = await _cache.GetAsync<EventDto[]>(EventsCacheConstants.TopEventsKey, ct);
        if (cached is not null)
        {
            return cached;
        }   

        var events = await _eventsRepository.GetTopEventsAsync(10, ct);
        var result = events.Select(EventDto.FromDomain).ToArray();
        await _cache.SetAsync(EventsCacheConstants.TopEventsKey, result, _config.TopEventsTtl, ct);

        return result;
    }

    public async Task<EventDto> GetEventAsync(Guid id, CancellationToken ct = default)
    {
        var cacheKey = EventsCacheConstants.EventKey(id);
        var cached = await _cache.GetAsync<EventDto>(cacheKey, ct);
        if (cached is not null)
        {
            return cached;
        }

        var domainEvent = await _eventsRepository.GetEventByIdAsync(id);

        if (domainEvent is null)
        {
            throw new NotFoundException($"Event Id: {id} not found");
        }

        var eventDto = EventDto.FromDomain(domainEvent);
        await _cache.SetAsync(cacheKey, eventDto, _config.EventTtl, ct);

        return eventDto;
    }

    public async Task<EventDto> AddEventAsync(CreateEventRequestDto item, CancellationToken ct = default)
    {
        if (item.TotalSeats <= 0)
        {
            throw new ValidationException("Общее количество мест должно быть больше 0");
        }

        var newEvent = new Event(item.Title, item.Description, item.StartAt, item.EndAt, item.TotalSeats);
        await _eventsRepository.AddEventAsync(newEvent, ct);
        await _eventsRepository.SaveChangesAsync(ct);

        await _cache.RemoveAsync(EventsCacheConstants.TopEventsKey, ct);

        return EventDto.FromDomain(newEvent);
    }

    public async Task<EventDto> ChangeEventAsync(Guid id, UpdateEventRequestDto item, CancellationToken ct = default)
    {
        var savedEvent = await _eventsRepository.GetEventByIdAsync(id);

        if (savedEvent is null)
        {
            throw new NotFoundException($"Event Id: {id} not found");
        }

        savedEvent.Title = item.Title;
        savedEvent.Description = item.Description;
        savedEvent.StartAt = item.StartAt;
        savedEvent.EndAt = item.EndAt;

        await _eventsRepository.ChangeEventAsync(savedEvent, ct);
        await _eventsRepository.SaveChangesAsync(ct);

        var eventDto = EventDto.FromDomain(savedEvent);

        await _cache.RemoveAsync(EventsCacheConstants.EventKey(id), ct);
        await _cache.RemoveAsync(EventsCacheConstants.TopEventsKey, ct);

        return eventDto;
    }

    public async Task<bool> RemoveEventAsync(Guid id, CancellationToken ct = default)
    {
        var domainEvent = await _eventsRepository.GetEventByIdAsync(id);

        if (domainEvent is null)
        {
            return false;
        }

        await _eventsRepository.RemoveEventAsync(id);
        await _eventsRepository.SaveChangesAsync();

        await _cache.RemoveAsync(EventsCacheConstants.EventKey(id), ct);
        await _cache.RemoveAsync(EventsCacheConstants.TopEventsKey, ct);

        return true;
    }
}
