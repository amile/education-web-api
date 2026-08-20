using System.Text.Json.Serialization;

namespace EducationWebApi.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{
    User,
    Admin,
}