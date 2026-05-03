using System.Diagnostics.CodeAnalysis;

namespace EducationWebApi;

public class EventDto
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required DateTime StartAt { get; set; }
    public required DateTime EndAt { get; set; }

    [SetsRequiredMembers]
    public EventDto(Guid id, string title, string? description, DateTime startAt, DateTime endAt)
    {
        Id = id;
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
    }
}