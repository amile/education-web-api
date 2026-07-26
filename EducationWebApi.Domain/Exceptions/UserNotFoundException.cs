namespace EducationWebApi.Domain;

public class UserNotFoundExeption : NotFoundException
{
    public UserNotFoundExeption() : base($"Incorrect login or password")
    {}
}
