using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Events.Enums;

namespace VibraHeka.Domain.Events.Entities;

public class EventEntity : ProductEntity
{
    public string EventID { get => ID; set => ID = value; }
    public DateTimeOffset EventDateUtc { get; set; } = DateTime.UtcNow;
    public int Duration { get; set; } = 0;
    public string EventPassword { get; set; } = string.Empty;
    public string EventTimezone { get; set; } = string.Empty;
    public EventStatus Status { get; set; } = EventStatus.MissingLink;

    public string EventLink { get; set; } = string.Empty;

}
