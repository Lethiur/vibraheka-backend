// Tests have been migrated to:
// - FromDomainTest.cs  (FromDomain suite)
// - FromDbModelTest.cs (FromDbModel suite)
// This file is intentionally empty.
/*
using System.ComponentModel;
using NUnit.Framework;
using VibraHeka.Domain.Recordings.Entities;
using VibraHeka.Domain.Recordings.Enums;
using VibraHeka.Infrastructure.Mappers;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.UnitTests.Mappers.RecordingEntityMapper;

[TestFixture]
public sealed class RecordingEntityMapperTests
{
    private Mappers.RecordingEntityMapper Mapper = default!;

    [SetUp]
    public void SetUp()
    {
        Mapper = new Mappers.RecordingEntityMapper();
    }

    [Test]
    [DisplayName("Should map all fields from RecordingEntity to RecordingDBModel correctly")]
    public void ShouldMapAllFieldsFromDomainToDbModel()
    {
        // Given: a fully populated RecordingEntity
        DateTimeOffset now = new(2025, 6, 10, 8, 0, 0, TimeSpan.Zero);
        RecordingEntity entity = new()
        {
            Id = "domain-id-1",
            Name = "Meditacion matutina",
            Description = "Sesion guiada de meditacion",
            Type = RecordingType.Meditacion,
            StorageKey = "recordings/domain-id-1/file.mp4",
            Created = now,
            CreatedBy = "admin-user",
            LastModified = now,
            LastModifiedBy = "admin-user"
        };

        // When: the mapper converts the entity to a DB model
        RecordingDBModel model = Mapper.FromDomain(entity);

        // Then: all fields are correctly mapped
        Assert.That(model.Id, Is.EqualTo(entity.Id),
            $"Expected Id '{entity.Id}' but got '{model.Id}'");
        Assert.That(model.Name, Is.EqualTo(entity.Name),
            $"Expected Name '{entity.Name}' but got '{model.Name}'");
        Assert.That(model.Description, Is.EqualTo(entity.Description),
            $"Expected Description '{entity.Description}' but got '{model.Description}'");
        Assert.That(model.Type, Is.EqualTo(entity.Type),
            $"Expected Type '{entity.Type}' but got '{model.Type}'");
        Assert.That(model.StorageKey, Is.EqualTo(entity.StorageKey),
            $"Expected StorageKey '{entity.StorageKey}' but got '{model.StorageKey}'");
        Assert.That(model.Created, Is.EqualTo(entity.Created),
            $"Expected Created '{entity.Created}' but got '{model.Created}'");
        Assert.That(model.CreatedBy, Is.EqualTo(entity.CreatedBy),
            $"Expected CreatedBy '{entity.CreatedBy}' but got '{model.CreatedBy}'");
        Assert.That(model.LastModified, Is.EqualTo(entity.LastModified),
            $"Expected LastModified '{entity.LastModified}' but got '{model.LastModified}'");
        Assert.That(model.LastModifiedBy, Is.EqualTo(entity.LastModifiedBy),
            $"Expected LastModifiedBy '{entity.LastModifiedBy}' but got '{model.LastModifiedBy}'");
    }

    [Test]
    [DisplayName("Should map Masterclass RecordingType correctly")]
    public void ShouldMapMasterclassTypeCorrectly()
    {
        // Given: an entity with type Masterclass
        RecordingEntity entity = new()
        {
            Id = "masterclass-id",
            Name = "Masterclass yoga",
            Description = "Yoga avanzado",
            Type = RecordingType.Masterclass,
            StorageKey = "recordings/masterclass-id/yoga.mp4",
            Created = DateTimeOffset.UtcNow
        };

        // When: the mapper converts the entity
        RecordingDBModel model = Mapper.FromDomain(entity);

        // Then: the type is correctly mapped
        Assert.That(model.Type, Is.EqualTo(RecordingType.Masterclass),
            $"Expected Type 'Masterclass' but got '{model.Type}'");
    }

    [Test]
    [DisplayName("Should map Taller RecordingType correctly")]
    public void ShouldMapTallerTypeCorrectly()
    {
        // Given: an entity with type Taller
        RecordingEntity entity = new()
        {
            Id = "taller-id",
            Name = "Taller respiracion",
            Description = "Respiracion consciente",
            Type = RecordingType.Taller,
            StorageKey = "recordings/taller-id/resp.mp4",
            Created = DateTimeOffset.UtcNow
        };

        // When: the mapper converts the entity
        RecordingDBModel model = Mapper.FromDomain(entity);

        // Then: the type is correctly mapped
        Assert.That(model.Type, Is.EqualTo(RecordingType.Taller),
            $"Expected Type 'Taller' but got '{model.Type}'");
    }
}

[TestFixture]
[Category("Unit")]
public sealed class FromDbModel : RecordingEntityMapperTests
{
    private Mappers.RecordingEntityMapper Mapper = default!;

    [SetUp]
    public void SetUp()
    {
        Mapper = new Mappers.RecordingEntityMapper();
    }

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
            Type = RecordingType.Taller,
            StorageKey = "recordings/db-model-id-1/taller.mp4",
            Created = now,
            CreatedBy = "admin-user",
            LastModified = now,
            LastModifiedBy = "admin-user"
        };

        // When: the mapper converts the DB model to a domain entity
        RecordingEntity entity = Mapper.FromDbModel(model);

        // Then: all fields are correctly mapped
        Assert.That(entity.Id, Is.EqualTo(model.Id),
            $"Expected Id '{model.Id}' but got '{entity.Id}'");
        Assert.That(entity.Name, Is.EqualTo(model.Name),
            $"Expected Name '{model.Name}' but got '{entity.Name}'");
        Assert.That(entity.Description, Is.EqualTo(model.Description),
            $"Expected Description '{model.Description}' but got '{entity.Description}'");
        Assert.That(entity.Type, Is.EqualTo(model.Type),
            $"Expected Type '{model.Type}' but got '{entity.Type}'");
        Assert.That(entity.StorageKey, Is.EqualTo(model.StorageKey),
            $"Expected StorageKey '{model.StorageKey}' but got '{entity.StorageKey}'");
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
            Id = "roundtrip-id",
            Name = "Meditacion",
            Description = "Sesion guiada",
            Type = RecordingType.Meditacion,
            StorageKey = "recordings/roundtrip-id/file.mp4",
            Created = now,
            CreatedBy = "admin",
            LastModified = now,
            LastModifiedBy = "admin"
        };

        // When: entity → DB model → entity
        RecordingDBModel dbModel = Mapper.FromDomain(originalEntity);
        RecordingEntity restoredEntity = Mapper.FromDbModel(dbModel);

        // Then: all fields of the restored entity match the original
        Assert.That(restoredEntity.Id, Is.EqualTo(originalEntity.Id),
            $"Round-trip Id mismatch: expected '{originalEntity.Id}' but got '{restoredEntity.Id}'");
        Assert.That(restoredEntity.Name, Is.EqualTo(originalEntity.Name),
            $"Round-trip Name mismatch: expected '{originalEntity.Name}' but got '{restoredEntity.Name}'");
        Assert.That(restoredEntity.Description, Is.EqualTo(originalEntity.Description),
            $"Round-trip Description mismatch");
        Assert.That(restoredEntity.Type, Is.EqualTo(originalEntity.Type),
            $"Round-trip Type mismatch: expected '{originalEntity.Type}' but got '{restoredEntity.Type}'");
        Assert.That(restoredEntity.StorageKey, Is.EqualTo(originalEntity.StorageKey),
            $"Round-trip StorageKey mismatch");
        Assert.That(restoredEntity.Created, Is.EqualTo(originalEntity.Created),
            $"Round-trip Created mismatch");
        Assert.That(restoredEntity.CreatedBy, Is.EqualTo(originalEntity.CreatedBy),
            $"Round-trip CreatedBy mismatch");
    }
}
*/
