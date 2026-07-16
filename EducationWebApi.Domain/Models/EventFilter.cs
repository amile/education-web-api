namespace EducationWebApi.Domain;

public record EventFilter(
    string? Title = null,
    DateTime? From = null,
    DateTime? To = null
);
