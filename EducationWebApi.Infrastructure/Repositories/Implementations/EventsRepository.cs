using System.Data;
using EducationWebApi.Application;
using EducationWebApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace EducationWebApi.Infrastructure;

public class EventsRepository : IEventsRepository
{
    private readonly AppDbContext _dbContext;

    public EventsRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginatedResult<Event>> GetAllEventsAsync(EventFilter filter, PagingRequest pagingRequest, CancellationToken ct = default)
    {
        var filteredItems = _dbContext.Events.AsQueryable();
            
        if (!string.IsNullOrWhiteSpace(filter.Title))
        {
            filteredItems = filteredItems.Where(item => item.Title.ToLower().Contains(filter.Title.ToLower()));
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
            .Select(EventFactory.FromDb)
            .ToArray();
    
        return new PaginatedResult<Event>(items, filteredItemsResult.Count, pagingRequest.Page, items.Length);
    }

    public async Task<Event?> GetEventByIdAsync(Guid id, CancellationToken ct = default)
    {
        var dbEvent = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == id, ct);

        return dbEvent is not null ? EventFactory.FromDb(dbEvent) : null;
    }

    public Task AddEventAsync(Event item, CancellationToken ct = default)
    {
        return _dbContext.AddAsync(EventFactory.ToDb(item), ct).AsTask();
    }

    public async Task ChangeEventAsync(Event item, CancellationToken ct = default)
    {
        var dbEvent = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == item.Id, ct);

        if (dbEvent is null)
        {
            throw new NotFoundException($"Event Id: {item.Id} not found");
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
            throw new NotFoundException($"Event Id: {id} not found");
        }

        _dbContext.Events.Remove(dbEvent);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _dbContext.SaveChangesAsync(ct);
    }
}
