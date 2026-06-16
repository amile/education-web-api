namespace EducationWebApi;

public interface IEventsService
{
    PaginatedResultDto<EventDto> GetEvents(EventFilterDto filter, PagingRequestDto pagingRequest);
    EventDto GetEvent(Guid id);
    EventDto AddEvent(CreateEventRequestDto item);
    EventDto ChangeEvent(Guid id, UpdateEventRequestDto item);
    bool RemoveEvent(Guid id);
}
