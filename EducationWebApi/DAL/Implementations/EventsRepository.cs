using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace EducationWebApi;

public class EventsRepository : IEventsRepository
{
    private readonly ConcurrentDictionary<Guid, Event> Events = new ConcurrentDictionary<Guid, Event>();

    public IEnumerable<Event> GetAllEvents()
    {
        return Events.Values.AsEnumerable();
    }

    public bool TryGetEvent(Guid id, [NotNullWhen(true)] out Event? item)
    {
        return Events.TryGetValue(id, out item);
    }

    public bool TryAddEvent(Event item)
    {
        return Events.TryAdd(item.Id, item);
    }

    public bool TryChangeEvent(Event item)
    {
        if (!Events.ContainsKey(item.Id))
        {
            return false;
        }

        Events[item.Id] = item;
        return true;
    }

    public bool TryRemoveEvent(Guid id)
    {
        return Events.TryRemove(id, out var _);
    }
}
