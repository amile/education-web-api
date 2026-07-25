namespace EducationWebApi.Domain;

public class ExpiredEventExeption : Exception
{
    public ExpiredEventExeption() : base("This event is expired")
    {}

    public ExpiredEventExeption(string message) : base(message)
    {}

    public ExpiredEventExeption(string message, Exception inner) : base(message, inner)
    {}
}
