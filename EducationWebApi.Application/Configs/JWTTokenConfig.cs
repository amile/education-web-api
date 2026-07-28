public class JWTTokenConfig
{
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required int ExpiresMinutes { get; set; }
    public required string Secret { get; set; }

    public JWTTokenConfig()
    {}

    public JWTTokenConfig(string issuer, string audience, int expiresMinutes, string secret)
    {
        Issuer = issuer;
        Audience = audience;
        ExpiresMinutes = expiresMinutes;
        Secret = secret;
    }
}
