using System.Text.Json.Serialization;

namespace Users.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{
    User,
    Admin,
}