using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;

namespace VibraHeka.Application.Catalog.Models;

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
