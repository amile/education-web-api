using System.Text.Json.Serialization;

namespace Bookings.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BookingStatus
{
    Pending,
    Confirmed,
    Rejected,
    Cancelled,
}