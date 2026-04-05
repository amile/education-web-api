using System.Diagnostics.CodeAnalysis;

namespace EducationWebApi;

public class Event
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required DateTime StartAt { get; set; }
    public required DateTime EndAt { get; set; }

    [SetsRequiredMembers]
    public Event(Guid id, string title, string? description, DateTime startAt, DateTime endAt)
    {
        Id = id;
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
    }

    [SetsRequiredMembers]
    public Event(string title, string? description, DateTime startAt, DateTime endAt)
        : this(Guid.NewGuid(), title, description, startAt, endAt) {}

    public static Event FromApi(EventRequestDto item) => new Event(item.Title, item.Description, item.StartAt, item.EndAt);
    public EventDto ToApi() => new EventDto(Id, Title, Description, StartAt, EndAt);
}