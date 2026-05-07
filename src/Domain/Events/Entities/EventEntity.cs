using Bnaya.CodeGeneration.BuilderPatternGeneration;
using VibraHeka.Domain.Events.Enums;

namespace VibraHeka.Domain.Events.Entities;

[GenerateBuilderPattern]
public partial class EventEntity
{
    public String EventID { get; set; } = string.Empty;
    public String EventName { get; set; } = string.Empty;
    public String EventDescription { get; set; } = string.Empty;
    public DateTime EventDateUtc { get; set; } = DateTime.UtcNow;
    public int Duration { get; set; } = 0;
    public String EventPassword { get; set; } = string.Empty;
    public String EventTimezone { get; set; } = string.Empty;
    public List<EventAttendee> Attendees { get; set; } = [];
    public EventStatus Status { get; set; } = EventStatus.MissingLink;
}
