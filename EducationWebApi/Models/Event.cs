using System.Diagnostics.CodeAnalysis;
using System.ComponentModel.DataAnnotations;
using EducationWebApi.DAL;

namespace EducationWebApi;

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

    public static Event CreateFromApi(CreateEventRequestDto item)
    {
        if (item.TotalSeats <= 0)
        {
            throw new ValidationException("Общее количество мест должно быть больше 0");
        }

        return new Event(item.Title, item.Description, item.StartAt, item.EndAt, item.TotalSeats);
    }

    public EventDto ToApi() => new EventDto(Id, Title, Description, StartAt, EndAt, TotalSeats, AvailableSeats);

    public static Event FromDb(EventEntity dbModel) => new Event()
    {
        Id = dbModel.Id, 
        Title = dbModel.Title, 
        Description = dbModel.Description, 
        StartAt = dbModel.StartAt, 
        EndAt = dbModel.EndAt, 
        TotalSeats = dbModel.TotalSeats, 
        AvailableSeats = dbModel.AvailableSeats
    };

    public EventEntity ToDb() => new EventEntity()
    {
        Id = Id,
        Title = Title,
        Description = Description,
        StartAt = StartAt,
        EndAt = EndAt,
        TotalSeats = TotalSeats,
        AvailableSeats = AvailableSeats
    };

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