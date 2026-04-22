using VibraHeka.Domain.Recordings.Enums;

namespace VibraHeka.Domain.Recordings.Entities;

public class RecordingEntity : BaseAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RecordingType Type { get; set; }

    public RecordingState State { get; set; } = RecordingState.Active;

    public bool IsActive()
    {
        return State == RecordingState.Active;
    }
}
