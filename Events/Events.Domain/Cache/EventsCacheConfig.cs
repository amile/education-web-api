namespace Events.Domain;

public class EventsCacheConfig
{
    public TimeSpan EventTtl { get; set; } = TimeSpan.FromSeconds(20);
    public TimeSpan TopEventsTtl { get; set; } = TimeSpan.FromSeconds(10);

    public EventsCacheConfig()
    {}

    public EventsCacheConfig(int eventTtl, int topEventsTtl)
    {
        EventTtl = TimeSpan.FromMinutes(eventTtl);
        TopEventsTtl = TimeSpan.FromMinutes(topEventsTtl);
    }
}