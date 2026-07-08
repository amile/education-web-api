using System.Data;
using EducationWebApi.DAL;
using Microsoft.EntityFrameworkCore;

namespace EducationWebApi;

public class EventsRepository : IEventsRepository
{
    private readonly AppDbContext _dbContext;

    public EventsRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginatedResultDto<Event>> GetAllEventsAsync(EventFilterDto filter, PagingRequestDto pagingRequest, CancellationToken ct = default)
    {
        var filteredItems = _dbContext.Events.AsQueryable();
            
        if (!string.IsNullOrWhiteSpace(filter.Title))
        {
            filteredItems = filteredItems.Where(item => item.Title.Contains(filter.Title, StringComparison.OrdinalIgnoreCase));
        }

        if (filter.From is not null)
        {
            filteredItems = filteredItems.Where(item => item.StartAt >= filter.From);
        }

        if (filter.To is not null)
        {
            filteredItems = filteredItems.Where(item => item.EndAt <= filter.To);
        }

        var filteredItemsResult = await filteredItems.ToListAsync(ct);

        var items = filteredItemsResult
            .OrderBy(item => item.StartAt)
            .Skip((pagingRequest.Page - 1) * pagingRequest.PageSize)
            .Take(pagingRequest.PageSize)
            .Select(item => Event.FromDb(item))
            .ToArray();
    
        return new PaginatedResultDto<Event>(items, filteredItemsResult.Count, pagingRequest.Page, items.Length);
    }

    public async Task<Event?> GetEventByIdAsync(Guid id, CancellationToken ct = default)
    {
        var dbEvent = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == id, ct);

        return dbEvent is not null ? Event.FromDb(dbEvent) : null;
    }

    public Task AddEventAsync(Event item, CancellationToken ct = default)
    {
        return _dbContext.AddAsync(item.ToDb(), ct).AsTask();
    }

    public async Task ChangeEventAsync(Event item, CancellationToken ct = default)
    {
        var dbEvent = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == item.Id, ct);

        if (dbEvent is null)
        {
            throw new KeyNotFoundException($"Event Id: {item.Id} not found");
        }

        dbEvent.Title = item.Title;
        dbEvent.Description = item.Description;
        dbEvent.StartAt = item.StartAt;
        dbEvent.EndAt = item.EndAt;
        dbEvent.TotalSeats = item.TotalSeats;
        dbEvent.AvailableSeats = item.AvailableSeats;

        _dbContext.Events.Update(dbEvent);
    }

    public async Task RemoveEventAsync(Guid id, CancellationToken ct = default)
    {
        var dbEvent = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == id, ct);

        if (dbEvent is null)
        {
            throw new KeyNotFoundException($"Event Id: {id} not found");
        }

        _dbContext.Events.Remove(dbEvent);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _dbContext.SaveChangesAsync(ct);
    }
}
