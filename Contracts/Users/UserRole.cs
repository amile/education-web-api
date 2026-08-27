using System.Text.Json.Serialization;

namespace Contracts;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{
    User,
    Admin,
}