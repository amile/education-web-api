namespace EducationWebApi.Domain;

public class NoPermissionException : Exception
{
    public NoPermissionException() : base("No permission to perform the requested action")
    {}

    public NoPermissionException(string message) : base(message)
    {}

    public NoPermissionException(string message, Exception inner) : base(message, inner)
    {}
}
