using System.Diagnostics.CodeAnalysis;

namespace EducationWebApi;

public interface IEventsRepository
{
    Task<IEnumerable<Event>> GetAllEvents();
    Task<Event?> TryGetEvent(Guid id);
    Task AddEvent(Event item);
    Task<bool> TryChangeEvent(Event item);
    Task<bool> TryRemoveEvent(Guid id);
    Task SaveChanges();
} 