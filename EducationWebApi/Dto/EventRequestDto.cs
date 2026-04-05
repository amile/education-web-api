using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EducationWebApi;

public class EventRequestDto : IValidatableObject
{
    [Required(ErrorMessage = "Наименование события обязательно для заполнения.")]
    public required string Title { get; set; }
    public string? Description { get; set; }

    [Required(ErrorMessage = "Дата начала события обязательна для заполнения.")]
    public required DateTime StartAt { get; set; }

    [Required(ErrorMessage = "Дата окончания события обязательна для заполнения.")]
    public required DateTime EndAt { get; set; }
    
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartAt >= EndAt)
        {
            yield return new ValidationResult(
                $"Дата окончания {EndAt} должна быть позже даты начала {StartAt}.",
                new[] { nameof(StartAt), nameof(EndAt) });
        }
    }
}