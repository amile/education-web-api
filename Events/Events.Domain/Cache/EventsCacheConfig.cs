namespace Events.Domain;

public class EventsCacheConfig
{
    public TimeSpan EventTtl { get; set; }
    public TimeSpan TopEventsTtl { get; set; }

    public EventsCacheConfig()
    {}

    public EventsCacheConfig(int eventTtl = 10, int topEventsTtl = 5)
    {
        EventTtl = TimeSpan.FromMinutes(eventTtl);
        TopEventsTtl = TimeSpan.FromMinutes(topEventsTtl);
    }
}