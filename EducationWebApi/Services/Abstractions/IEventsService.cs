namespace EducationWebApi;

public interface IEventsService
{
    PaginatedResultDto<EventDto> GetEvents(EventFilterDto filter, PagingRequestDto pagingRequest);
    EventDto GetEvent(Guid id);
    EventDto AddEvent(EventRequestDto item);
    EventDto ChangeEvent(Guid id, EventRequestDto item);
    bool RemoveEvent(Guid id);
}
