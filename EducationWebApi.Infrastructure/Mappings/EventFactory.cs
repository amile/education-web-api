using EducationWebApi.Domain;

namespace EducationWebApi.Infrastructure;

public class EventFactory
{
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

    public static EventEntity ToDb(Event model) => new EventEntity()
    {
        Id = model.Id,
        Title = model.Title,
        Description = model.Description,
        StartAt = model.StartAt,
        EndAt = model.EndAt,
        TotalSeats = model.TotalSeats,
        AvailableSeats = model.AvailableSeats
    };
}