using System.Collections.Concurrent;
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

    public async Task<IEnumerable<Event>> GetAllEvents()
    {
        return _dbContext.Events.Select(item => Event.FromDb(item));
    }

    public async Task<Event?> TryGetEvent(Guid id)
    {
        var dbEvent = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == id);

        return dbEvent is not null ? Event.FromDb(dbEvent) : null;
    }

    public async Task AddEvent(Event item)
    {
        await _dbContext.AddAsync(item.ToDb());
    }

    public async Task<bool> TryChangeEvent(Event item)
    {
        var dbEvent = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == item.Id);

        if (dbEvent is null)
        {
            return false;
        }

        dbEvent.Title = item.Title;
        dbEvent.Description = item.Description;
        dbEvent.StartAt = item.StartAt;
        dbEvent.EndAt = item.EndAt;
        dbEvent.TotalSeats = item.TotalSeats;
        dbEvent.AvailableSeats = item.AvailableSeats;

        _dbContext.Events.Update(dbEvent);
        return true;
    }

    public async Task<bool> TryRemoveEvent(Guid id)
    {
        var dbEvent = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == id);

        if (dbEvent is null)
        {
            return false;
        }

        _dbContext.Events.Remove(dbEvent);

        return true;
    }

    public async Task SaveChanges()
    {
        await _dbContext.SaveChangesAsync();
    }
}
