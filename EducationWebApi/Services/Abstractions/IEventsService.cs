namespace EducationWebApi;

public interface IEventsService
{
    Task<PaginatedResultDto<EventDto>> GetEventsAsync(EventFilterDto filter, PagingRequestDto pagingRequest, CancellationToken ct = default);
    Task<EventDto> GetEventAsync(Guid id, CancellationToken ct = default);
    Task<EventDto> AddEventAsync(CreateEventRequestDto item, CancellationToken ct = default);
    Task<EventDto> ChangeEventAsync(Guid id, UpdateEventRequestDto item, CancellationToken ct = default);
    Task<bool> RemoveEventAsync(Guid id, CancellationToken ct = default);
}
