namespace EducationWebApi.Domain;

public class TooManyBookingsException : Exception
{
    public TooManyBookingsException() : base("User has too many bookings")
    {}

    public TooManyBookingsException(string message) : base(message)
    {}

    public TooManyBookingsException(string message, Exception inner) : base(message, inner)
    {}
}
