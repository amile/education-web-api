using System.Data;
using Microsoft.AspNetCore.Http.Features;

namespace EducationWebApi;

public class EventsService : IEventsService
{
    public static List<Event> Events { get; set; } = [];

    public List<EventDto> GetEvents()
    {
        return Events.Select(item => item.ToApi()).ToList();
    }

    public EventDto? GetEvent(Guid id)
    {
        return Events.FirstOrDefault(item => item.Id == id)?.ToApi();
    }

    public void AddEvent(EventRequestDto item)
    {
        Events.Add(Event.FromApi(item));
    }

    public bool ChangeEvent(Guid id, EventRequestDto item)
    {
        var index = Events.FindIndex(_item => _item.Id == id);

        if (index == -1)
        {
            return false;
        }

        Events[index] = new Event(id, item.Title, item.Description, item.StartAt, item.EndAt);
        return true;
    }

    public bool RemoveEvent(Guid id)
    {
        var index = Events.FindIndex(_item => _item.Id == id);

        if (index == -1)
        {
            return false;
        }

        Events.RemoveAt(index);
        return true;
    }
}
