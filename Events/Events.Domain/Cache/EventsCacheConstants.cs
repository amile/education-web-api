namespace Events.Domain;

public static class EventsCacheConstants
{
    public const string EventsCacheSectionName = "EventsCache";
    public const string TopEventsKey = "events:top10";
    public static string EventKey(Guid id) => $"event:{id}";
}