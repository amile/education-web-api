using System.Diagnostics.CodeAnalysis;
using EducationWebApi.Domain;

namespace EducationWebApi.Application;

public class EventDto
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required DateTime StartAt { get; set; }
    public required DateTime EndAt { get; set; }
    public required int TotalSeats { get; set; }
    public required int AvailableSeats { get; set; }

    [SetsRequiredMembers]
    public EventDto(Guid id, string title, string? description, DateTime startAt, DateTime endAt, int totalSeats, int availableSeats)
    {
        Id = id;
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
        TotalSeats = totalSeats;
        AvailableSeats = availableSeats;
    }

    public static EventDto FromDomain(Event model) => new EventDto(
        model.Id, model.Title, model.Description, model.StartAt, model.EndAt, model.TotalSeats, model.AvailableSeats);
}