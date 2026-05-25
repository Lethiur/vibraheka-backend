using Riok.Mapperly.Abstractions;
using VibraHeka.Domain.Recordings.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.Mappers;

[Mapper]
public partial class RecordingEntityMapper
{
    [MapProperty(nameof(RecordingEntity.RecordingID), nameof(RecordingDBModel.Id))]
    public partial RecordingDBModel FromDomain(RecordingEntity entity);

    [MapProperty(nameof(RecordingDBModel.Id), nameof(RecordingEntity.RecordingID))]
    public partial RecordingEntity FromDbModel(RecordingDBModel model);
}
