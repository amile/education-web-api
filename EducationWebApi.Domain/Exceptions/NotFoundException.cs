namespace EducationWebApi.Domain;

public class NotFoundException : Exception
{
    public NotFoundException() : base("Requested item not found")
    {}

    public NotFoundException(string message) : base(message)
    {}

    public NotFoundException(string message, Exception inner) : base(message, inner)
    {}
}
