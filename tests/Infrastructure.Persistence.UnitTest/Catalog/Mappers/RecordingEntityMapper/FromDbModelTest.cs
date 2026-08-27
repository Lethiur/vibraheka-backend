using System.ComponentModel;
using Infrastructure.Persistence.Catalog.Models;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.Persistence.UnitTest.Catalog.Mappers.RecordingEntityMapper;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class FromDbModelTest : GenericRecordingEntityMapperTest
{
    [Test]
    [DisplayName("Should map all fields from RecordingDBModel to RecordingEntity correctly")]
    public void ShouldMapAllFieldsFromDbModelToDomain()
    {
        // Given: a fully populated RecordingDBModel
        DateTimeOffset now = new(2025, 6, 10, 8, 0, 0, TimeSpan.Zero);
        RecordingDBModel model = new()
        {
            Id = "db-model-id-1",
            Name = "Taller de respiracion",
            Description = "Tecnicas avanzadas",
            RecordingType = RecordingType.Taller,
            Created = now,
            CreatedBy = "admin-user",
            LastModified = now,
            LastModifiedBy = "admin-user"
        };

        // When: the mapper converts the DB model to a domain entity
        RecordingEntity entity = Mapper.FromDbModel(model);

        // Then: all fields are correctly mapped
        Assert.That(entity.RecordingID, Is.EqualTo(model.Id),
            $"Expected Id '{model.Id}' but got '{entity.RecordingID}'");
        Assert.That(entity.Name, Is.EqualTo(model.Name),
            $"Expected Name '{model.Name}' but got '{entity.Name}'");
        Assert.That(entity.Description, Is.EqualTo(model.Description),
            $"Expected Description '{model.Description}' but got '{entity.Description}'");
        Assert.That(entity.RecordingType, Is.EqualTo(model.RecordingType),
            $"Expected Type '{model.RecordingType}' but got '{entity.RecordingType}'");
        Assert.That(entity.Created, Is.EqualTo(model.Created),
            $"Expected Created '{model.Created}' but got '{entity.Created}'");
        Assert.That(entity.CreatedBy, Is.EqualTo(model.CreatedBy),
            $"Expected CreatedBy '{model.CreatedBy}' but got '{entity.CreatedBy}'");
        Assert.That(entity.LastModified, Is.EqualTo(model.LastModified),
            $"Expected LastModified '{model.LastModified}' but got '{entity.LastModified}'");
        Assert.That(entity.LastModifiedBy, Is.EqualTo(model.LastModifiedBy),
            $"Expected LastModifiedBy '{model.LastModifiedBy}' but got '{entity.LastModifiedBy}'");
    }

    [Test]
    [DisplayName("Should produce entity that round-trips back to identical DB model")]
    public void ShouldRoundTripFromDomainToDbModelAndBack()
    {
        // Given: a RecordingEntity
        DateTimeOffset now = new(2025, 3, 20, 12, 0, 0, TimeSpan.Zero);
        RecordingEntity originalEntity = new()
        {
            RecordingID = "roundtrip-id",
            Name = "Meditacion",
            Description = "Sesion guiada",
            RecordingType = RecordingType.Meditacion,
            Created = now,
            CreatedBy = "admin",
            LastModified = now,
            LastModifiedBy = "admin"
        };

        // When: entity → DB model → entity
        RecordingDBModel dbModel = Mapper.FromDomain(originalEntity);
        RecordingEntity restoredEntity = Mapper.FromDbModel(dbModel);

        // Then: all fields of the restored entity match the original
        Assert.That(restoredEntity.RecordingID, Is.EqualTo(originalEntity.RecordingID),
            $"Round-trip Id mismatch: expected '{originalEntity.RecordingID}' but got '{restoredEntity.RecordingID}'");
        Assert.That(restoredEntity.Name, Is.EqualTo(originalEntity.Name),
            $"Round-trip Name mismatch: expected '{originalEntity.Name}' but got '{restoredEntity.Name}'");
        Assert.That(restoredEntity.Description, Is.EqualTo(originalEntity.Description),
            "Round-trip Description mismatch");
        Assert.That(restoredEntity.RecordingType, Is.EqualTo(originalEntity.RecordingType),
            $"Round-trip Type mismatch: expected '{originalEntity.RecordingType}' but got '{restoredEntity.RecordingType}'");
        Assert.That(restoredEntity.Created, Is.EqualTo(originalEntity.Created),
            "Round-trip Created mismatch");
        Assert.That(restoredEntity.CreatedBy, Is.EqualTo(originalEntity.CreatedBy),
            "Round-trip CreatedBy mismatch");
    }
}


