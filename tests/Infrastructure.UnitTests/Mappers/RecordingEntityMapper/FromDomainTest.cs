using System.ComponentModel;
using VibraHeka.Domain.Recordings.Entities;
using VibraHeka.Domain.Recordings.Enums;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.UnitTests.Mappers.RecordingEntityMapper;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class FromDomainTest : GenericRecordingEntityMapperTest
{
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
            Created = DateTimeOffset.UtcNow
        };

        // When: the mapper converts the entity
        RecordingDBModel model = Mapper.FromDomain(entity);

        // Then: the type is correctly mapped
        Assert.That(model.Type, Is.EqualTo(RecordingType.Taller),
            $"Expected Type 'Taller' but got '{model.Type}'");
    }
}


