using VibraHeka.Domain.Recordings.Entities;
using VibraHeka.Domain.Recordings.Enums;

namespace VibraHeka.Application.Recordings.Queries.GetAllRecordings;

public record RecordingDto(
    string Id,
    string Name,
    string Description,
    RecordingType Type,
    RecordingTier Tier,
    DateTimeOffset Created)
{
    public static RecordingDto FromDomain(RecordingEntity entity) =>
        new(entity.RecordingID, entity.Name, entity.Description, entity.RecordingType, entity.Tier, entity.Created);
}
