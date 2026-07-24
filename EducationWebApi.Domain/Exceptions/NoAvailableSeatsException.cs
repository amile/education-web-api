namespace EducationWebApi.Domain;

public class NoAvailableSeatsException : Exception
{
    public NoAvailableSeatsException() : base("No available seats for this event")
    {}

    public NoAvailableSeatsException(string message) : base(message)
    {}

    public NoAvailableSeatsException(string message, Exception inner) : base(message, inner)
    {}
}
