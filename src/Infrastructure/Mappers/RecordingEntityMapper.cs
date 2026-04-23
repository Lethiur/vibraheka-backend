using Riok.Mapperly.Abstractions;
using VibraHeka.Domain.Recordings.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.Mappers;

[Mapper]
public partial class RecordingEntityMapper
{
    public partial RecordingDBModel FromDomain(RecordingEntity entity);

    public partial RecordingEntity FromDbModel(RecordingDBModel model);
}
