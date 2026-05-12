namespace EducationWebApi;

public record EventFilterDto(
    string? Title = null,
    DateTime? From = null,
    DateTime? To = null
);