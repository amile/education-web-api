using EducationWebApi.Domain;

namespace EducationWebApi.Application;

public record EventFilterDto(
    string? Title = null,
    DateTime? From = null,
    DateTime? To = null
)
{
    public EventFilter ToDomain() => new(Title, From, To); 
}