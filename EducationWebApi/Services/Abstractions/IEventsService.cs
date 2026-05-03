namespace EducationWebApi;

public interface IEventsService
{
    List<EventDto> GetEvents();
    EventDto? GetEvent(Guid id);
    void AddEvent(EventRequestDto item);
    bool ChangeEvent(Guid id, EventRequestDto item);
    bool RemoveEvent(Guid id);
}
