using System.Data;
using Microsoft.AspNetCore.Http.Features;

namespace EducationWebApi;

public class EventsService : IEventsService
{
    public static List<Event> Events { get; set; } = [];

    public List<Event> GetEvents()
    {
        return Events;
    }

    public Event? GetEvent(Guid id)
    {
        return Events.FirstOrDefault(item => item.Id == id);
    }

    public void AddEvent(Event item)
    {
        Events.Add(item);
    }

    public bool ChangeEvent(Event item, out bool success)
    {
        success = true;
        var index = Events.FindIndex(_item => _item.Id == item.Id);

        if (index == -1)
        {
            success = false;
        }

        Events[index] = item;
        return success;
    }

    public bool RemoveEvent(Guid id, out bool success)
    {
        success = true;
        var index = Events.FindIndex(_item => _item.Id == id);

        if (index == -1)
        {
            success = false;
        }

        Events.RemoveAt(index);
        return success;
    }
}
