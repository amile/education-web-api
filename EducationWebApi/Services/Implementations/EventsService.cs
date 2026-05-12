using System.Collections.Concurrent;
using System.Data;

namespace EducationWebApi;

public class EventsService : IEventsService
{
    public static ConcurrentDictionary<Guid, Event> Events { get; set; } = new ConcurrentDictionary<Guid, Event>();

    public PaginatedResultDto<EventDto> GetEvents(EventFilterDto filter, PagingRequestDto pagingRequest)
    {
        var filteredItems = Events.Values.AsEnumerable();
            
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
            .Skip((pagingRequest.Page - 1) * pagingRequest.PageSize)
            .Take(pagingRequest.PageSize)
            .Select(item => item.ToApi()).ToArray(); 

        int totalPages = (int)Math.Ceiling((double)filteredItemsResult.Count / pagingRequest.PageSize);
    
        return new PaginatedResultDto<EventDto>(items, filteredItemsResult.Count, pagingRequest.Page, items.Length);
    }

    public EventDto GetEvent(Guid id)
    {
        if (!Events.TryGetValue(id, out var item))
        {
            throw new KeyNotFoundException($"Event Id: {id} not found");
        }
        return item.ToApi();
    }

    public EventDto AddEvent(EventRequestDto item)
    {
        var newEvent = Event.FromApi(item);
        Events.TryAdd(newEvent.Id, newEvent);

        return newEvent.ToApi();
    }

    public EventDto ChangeEvent(Guid id, EventRequestDto item)
    {
        if (!Events.ContainsKey(id))
        {
            throw new KeyNotFoundException($"Event Id: {id} not found");
        }

        var newEvent = new Event(id, item.Title, item.Description, item.StartAt, item.EndAt);
        Events[id] = newEvent;
        return newEvent.ToApi();
    }

    public bool RemoveEvent(Guid id)
    {
        if (!Events.TryRemove(id, out var _))
        {
            return false;
        }

        return true;
    }
}
