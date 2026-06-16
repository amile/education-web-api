using System.Collections.Concurrent;
using System.Data;

namespace EducationWebApi;

public class EventsService : IEventsService
{
    private readonly IEventsRepository _eventsRepository;

    public EventsService(IEventsRepository eventsRepository)
    {
        _eventsRepository = eventsRepository;
    }

    public PaginatedResultDto<EventDto> GetEvents(EventFilterDto filter, PagingRequestDto pagingRequest)
    {
        var filteredItems = _eventsRepository.GetAllEvents();
            
        if (!string.IsNullOrWhiteSpace(filter.Title))
        {
            filteredItems = filteredItems.Where(item => item.Title.Contains(filter.Title, StringComparison.OrdinalIgnoreCase));
        }

        if (filter.From is not null)
        {
            filteredItems = filteredItems.Where(item => item.StartAt >= filter.From);
        }

        if (filter.To is not null)
        {
            filteredItems = filteredItems.Where(item => item.EndAt <= filter.To);
        }

        var filteredItemsResult = filteredItems.ToList();

        var items = filteredItemsResult
            .OrderBy(item => item.StartAt)
            .Skip((pagingRequest.Page - 1) * pagingRequest.PageSize)
            .Take(pagingRequest.PageSize)
            .Select(item => item.ToApi()).ToArray();
    
        return new PaginatedResultDto<EventDto>(items, filteredItemsResult.Count, pagingRequest.Page, items.Length);
    }

    public EventDto GetEvent(Guid id)
    {
        if (!_eventsRepository.TryGetEvent(id, out var item))
        {
            throw new KeyNotFoundException($"Event Id: {id} not found");
        }
        return item!.ToApi();
    }

    public EventDto AddEvent(CreateEventRequestDto item)
    {
        var newEvent = Event.CreateFromApi(item);
        _eventsRepository.TryAddEvent(newEvent);

        return newEvent.ToApi();
    }

    public EventDto ChangeEvent(Guid id, UpdateEventRequestDto item)
    {
        if (!_eventsRepository.TryGetEvent(id, out var oldEvent))
        {
            throw new KeyNotFoundException($"Event Id: {id} not found");
        }
        var newEvent = new Event(id, item.Title, item.Description, item.StartAt, item.EndAt, oldEvent.TotalSeats);
        _eventsRepository.TryChangeEvent(newEvent);

        return newEvent.ToApi();
    }

    public bool RemoveEvent(Guid id)
    {
        return _eventsRepository.TryRemoveEvent(id);
    }
}
