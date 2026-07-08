using System.Data;

namespace EducationWebApi;

public class EventsService : IEventsService
{
    private readonly IEventsRepository _eventsRepository;

    public EventsService(IEventsRepository eventsRepository)
    {
        _eventsRepository = eventsRepository;
    }

    public async Task<PaginatedResultDto<EventDto>> GetEventsAsync(EventFilterDto filter, PagingRequestDto pagingRequest, CancellationToken ct = default)
    {
        var result = await _eventsRepository.GetAllEventsAsync(filter, pagingRequest, ct);
        var data = result.Data.Select(item => item.ToApi()).ToArray();
    
        return new PaginatedResultDto<EventDto>(data, result.TotalCount, result.CurrentPage, result.PageSize);
    }

    public async Task<EventDto> GetEventAsync(Guid id, CancellationToken ct = default)
    {
        var domainEvent = await _eventsRepository.GetEventByIdAsync(id);

        if (domainEvent is null)
        {
            throw new KeyNotFoundException($"Event Id: {id} not found");
        }

        return domainEvent.ToApi();
    }

    public async Task<EventDto> AddEventAsync(CreateEventRequestDto item, CancellationToken ct = default)
    {
        var newEvent = Event.CreateFromApi(item);
        await _eventsRepository.AddEventAsync(newEvent, ct);
        await _eventsRepository.SaveChangesAsync(ct);

        return newEvent.ToApi();
    }

    public async Task<EventDto> ChangeEventAsync(Guid id, UpdateEventRequestDto item, CancellationToken ct = default)
    {
        var savedEvent = await _eventsRepository.GetEventByIdAsync(id);

        if (savedEvent is null)
        {
            throw new KeyNotFoundException($"Event Id: {id} not found");
        }

        var eventToChange = savedEvent.UpdateFromApi(item);
        await _eventsRepository.ChangeEventAsync(eventToChange, ct);
        await _eventsRepository.SaveChangesAsync(ct);

        return eventToChange.ToApi();
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

        return true;
    }
}
