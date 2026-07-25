namespace EducationWebApi.Domain;

public class NoPermissionExeption : Exception
{
    public NoPermissionExeption() : base("No permission to perform the requested action")
    {}

    public NoPermissionExeption(string message) : base(message)
    {}

    public NoPermissionExeption(string message, Exception inner) : base(message, inner)
    {}
}
