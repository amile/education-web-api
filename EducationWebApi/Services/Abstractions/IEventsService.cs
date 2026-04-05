namespace EducationWebApi;

public interface IEventsService
{
    List<Event> GetEvents();
    Event? GetEvent(Guid id);
    void AddEvent(Event item);
    bool ChangeEvent(Event item, out bool success);
    bool RemoveEvent(Guid id, out bool success);
}
