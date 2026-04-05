namespace EducationWebApi;

public class Event
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required DateTime StartAt { get; set; }
    public required DateTime EndAt { get; set;}
}