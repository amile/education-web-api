namespace EducationWebApi.Domain;

public class BookingAlreadyCancelledException : Exception
{
    public BookingAlreadyCancelledException() : base("Booking is already cancelled")
    {}

    public BookingAlreadyCancelledException(string message) : base(message)
    {}

    public BookingAlreadyCancelledException(string message, Exception inner) : base(message, inner)
    {}
}
