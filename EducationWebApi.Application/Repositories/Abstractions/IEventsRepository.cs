using EducationWebApi.Domain;

namespace EducationWebApi.Application;

public interface IEventsRepository
{
    Task<PaginatedResult<Event>> GetAllEventsAsync(EventFilter filter, PagingRequest pagingRequest, CancellationToken ct = default);
    Task<Event?> GetEventByIdAsync(Guid id, CancellationToken ct = default);
    Task AddEventAsync(Event item, CancellationToken ct = default);
    Task ChangeEventAsync(Event item, CancellationToken ct = default);
    Task RemoveEventAsync(Guid id, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
} 