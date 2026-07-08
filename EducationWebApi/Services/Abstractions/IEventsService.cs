namespace EducationWebApi;

public interface IEventsService
{
    Task<PaginatedResultDto<EventDto>> GetEventsAsync(EventFilterDto filter, PagingRequestDto pagingRequest);
    Task<EventDto> GetEventAsync(Guid id);
    Task<EventDto> AddEventAsync(CreateEventRequestDto item);
    Task<EventDto> ChangeEventAsync(Guid id, UpdateEventRequestDto item);
    Task<bool> RemoveEventAsync(Guid id);
}
