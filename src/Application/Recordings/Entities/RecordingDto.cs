using VibraHeka.Domain.Recordings.Entities;
using VibraHeka.Domain.Recordings.Enums;

namespace VibraHeka.Application.Recordings.Queries.GetAllRecordings;

public record RecordingDto(
    string Id,
    string Name,
    string Description,
    RecordingType Type,
    DateTimeOffset Created)
{
    public static RecordingDto FromDomain(RecordingEntity entity) =>
        new(entity.Id, entity.Name, entity.Description, entity.Type, entity.Created);
}

