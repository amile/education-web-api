using System.Diagnostics.CodeAnalysis;

namespace EducationWebApi.Domain;

public class Event
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required DateTime StartAt { get; set; }
    public required DateTime EndAt { get; set; }
    public required int TotalSeats { get; set; }
    public required int AvailableSeats { get; set; }


    public Event() {}

    [SetsRequiredMembers]
    public Event(Guid id, string title, string? description, DateTime startAt, DateTime endAt, int totalSeats)
    {
        Id = id;
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
        TotalSeats = totalSeats;
        AvailableSeats = totalSeats;
    }

    [SetsRequiredMembers]
    public Event(string title, string? description, DateTime startAt, DateTime endAt, int totalSeats)
        : this(Guid.NewGuid(), title, description, startAt, endAt, totalSeats) {}

    public bool TryReserveSeats(int count = 1)
    {
        var availableSeats = AvailableSeats - count;

        if (availableSeats < 0)
        {
            return false;
        }

        AvailableSeats = availableSeats;
        return true;
    }

    public void ReleaseSeats(int count = 1)
    {
        var availableSeats = AvailableSeats + count;

        if (availableSeats > TotalSeats)
        {
            AvailableSeats = TotalSeats;
        }
        else
        {
            AvailableSeats = availableSeats;
        }
    }
}