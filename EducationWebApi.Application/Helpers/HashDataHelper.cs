using System.Security.Cryptography;
using System.Text;

public static class HashDataHelper
{
    public static string GetHash(string key)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return Convert.ToHexString(bytes);
    }
}