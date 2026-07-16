namespace EducationWebApi.Application;

public class CreateEventRequestDto : BaseEventRequestDto
{
    public required int TotalSeats { get; set; }
}