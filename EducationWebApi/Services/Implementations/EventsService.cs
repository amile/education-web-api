using System.Data;
using EducationWebApi.DAL;
using Microsoft.EntityFrameworkCore;

namespace EducationWebApi;

public class EventsService : IEventsService
{
    private readonly AppDbContext _dbContext;

    public EventsService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginatedResultDto<EventDto>> GetEventsAsync(EventFilterDto filter, PagingRequestDto pagingRequest)
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

        var filteredItemsResult = filteredItems.ToList();

        var items = filteredItemsResult
            .OrderBy(item => item.StartAt)
            .Skip((pagingRequest.Page - 1) * pagingRequest.PageSize)
            .Take(pagingRequest.PageSize)
            .Select(item => Event.FromDb(item).ToApi())
            .ToArray();
    
        return new PaginatedResultDto<EventDto>(items, filteredItemsResult.Count, pagingRequest.Page, items.Length);
    }

    public async Task<EventDto> GetEventAsync(Guid id)
    {
        var dbEvent = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == id);

        if (dbEvent is null)
        {
            throw new KeyNotFoundException($"Event Id: {id} not found");
        }

        return Event.FromDb(dbEvent).ToApi();
    }

    public async Task<EventDto> AddEventAsync(CreateEventRequestDto item)
    {
        var newEvent = Event.CreateFromApi(item);
        await _dbContext.AddAsync(newEvent.ToDb());
        await _dbContext.SaveChangesAsync();

        return newEvent.ToApi();
    }

    public async Task<EventDto> ChangeEventAsync(Guid id, UpdateEventRequestDto item)
    {
        var dbEvent = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == id);

        if (dbEvent is null)
        {
            throw new KeyNotFoundException($"Event Id: {id} not found");
        }

        dbEvent.Title = item.Title;
        dbEvent.Description = item.Description;
        dbEvent.StartAt = item.StartAt;
        dbEvent.EndAt = item.EndAt;

        _dbContext.Events.Update(dbEvent);
        await _dbContext.SaveChangesAsync();

        return Event.FromDb(dbEvent).ToApi();
    }

    public async Task<bool> RemoveEventAsync(Guid id)
    {
        var dbEvent = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == id);

        if (dbEvent is null)
        {
            return false;
        }

        _dbContext.Events.Remove(dbEvent);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}
