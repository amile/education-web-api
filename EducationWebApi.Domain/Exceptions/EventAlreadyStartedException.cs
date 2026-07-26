namespace EducationWebApi.Domain;

public class EventAlreadyStartedException : Exception
{
    public EventAlreadyStartedException() : base("This event is already started")
    {}

    public EventAlreadyStartedException(string message) : base(message)
    {}

    public EventAlreadyStartedException(string message, Exception inner) : base(message, inner)
    {}
}
