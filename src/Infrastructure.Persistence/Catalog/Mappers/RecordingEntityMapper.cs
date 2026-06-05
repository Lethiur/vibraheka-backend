using Infrastructure.Persistence.Catalog.Models;
using Riok.Mapperly.Abstractions;
using VibraHeka.Domain.Catalog.Entities;

namespace Infrastructure.Persistence.Catalog.Mappers;

[Mapper]
public partial class RecordingEntityMapper
{
    [MapProperty(nameof(RecordingEntity.RecordingID), nameof(RecordingDBModel.Id))]
    public partial RecordingDBModel FromDomain(RecordingEntity entity);

    [MapProperty(nameof(RecordingDBModel.Id), nameof(RecordingEntity.RecordingID))]
    public partial RecordingEntity FromDbModel(RecordingDBModel model);
}
