namespace EducationWebApi;

public record EventFilter(
    string? Title = null,
    DateTime? From = null,
    DateTime? To = null
)
{
    public static EventFilter FromApi(EventFilterDto filter) => new(filter.Title, filter.From, filter.To); 
}