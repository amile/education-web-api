using System.Diagnostics.CodeAnalysis;

namespace EducationWebApi;

public interface IEventsRepository
{
    IEnumerable<Event> GetAllEvents();
    bool TryGetEvent(Guid id, [NotNullWhen(true)] out Event? item);
    bool TryAddEvent(Event item);
    bool TryChangeEvent(Event item);
    bool TryRemoveEvent(Guid id);
} 