using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Recordings.Enums;

namespace VibraHeka.Domain.Recordings.Entities;

public class RecordingEntity : ProductEntity
{
    public string RecordingID
    {
        get => ID;
        set => ID = value;
    }

    public RecordingTier Tier { get; set; } = RecordingTier.Free;

    public RecordingType RecordingType { get; set; } = RecordingType.Masterclass;

    public bool IsForSubscribers()
    {
        return Tier == RecordingTier.Premium;
    }
}
