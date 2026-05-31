using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace EducationWebApi;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BookingStatus
{
    Pending,
    Confirmed,
    Rejected
}