namespace EducationWebApi.Domain;

public class ExpiredEventException : Exception
{
    public ExpiredEventException() : base("This event is expired")
    {}

    public ExpiredEventException(string message) : base(message)
    {}

    public ExpiredEventException(string message, Exception inner) : base(message, inner)
    {}
}
