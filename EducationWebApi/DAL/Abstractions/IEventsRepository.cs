using System.Diagnostics.CodeAnalysis;

namespace EducationWebApi;

public interface IEventsRepository
{
    Task<PaginatedResultDto<Event>> GetAllEventsAsync(EventFilterDto filter, PagingRequestDto pagingRequest, CancellationToken ct = default);
    Task<Event?> GetEventByIdAsync(Guid id, CancellationToken ct = default);
    Task AddEventAsync(Event item, CancellationToken ct = default);
    Task ChangeEventAsync(Event item, CancellationToken ct = default);
    Task RemoveEventAsync(Guid id, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
} 