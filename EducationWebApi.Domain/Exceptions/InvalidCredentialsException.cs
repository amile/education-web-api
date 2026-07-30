namespace EducationWebApi.Domain;

public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException() : base($"Incorrect login or password")
    {}

    public InvalidCredentialsException(string message) : base(message)
    {}

    public InvalidCredentialsException(string message, Exception inner) : base(message, inner)
    {}
}
