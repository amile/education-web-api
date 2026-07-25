namespace EducationWebApi.Domain;

public class NotFoundExeption : Exception
{
    public NotFoundExeption() : base("Requested item not found")
    {}

    public NotFoundExeption(string message) : base(message)
    {}

    public NotFoundExeption(string message, Exception inner) : base(message, inner)
    {}
}
