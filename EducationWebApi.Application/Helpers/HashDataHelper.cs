using Microsoft.AspNetCore.Identity;

namespace EducationWebApi.Application.Helpers;

public static class HashDataHelper
{
    private static readonly PasswordHasher<string> _passwordHasher = new PasswordHasher<string>();

    public static string Hash(string login, string password)
    {
        return _passwordHasher.HashPassword(login, password);
    }

    public static bool Verify(string login, string password, string passwordHash)
    {
        var result = _passwordHasher.VerifyHashedPassword(login, passwordHash, password);

        return result == PasswordVerificationResult.Success;
    }
}