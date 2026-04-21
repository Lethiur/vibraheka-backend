using System.ComponentModel;
using NUnit.Framework;
using VibraHeka.Domain.Recordings.Entities;
using VibraHeka.Domain.Recordings.Enums;

namespace VibraHeka.Domain.UnitTests.Recordings.Entities;

[TestFixture]
public class RecordingEntityTest
{
    [Test]
    [DisplayName("Should create entity with default empty string values")]
    public void ShouldCreateEntityWithDefaultValues()
    {
        // Given / When: a new RecordingEntity is instantiated with no arguments
        RecordingEntity entity = new();

        // Then: all string properties should default to empty strings
        Assert.That(entity.Id, Is.EqualTo(string.Empty),
            $"Expected Id to default to empty string but got: '{entity.Id}'");
        Assert.That(entity.Name, Is.EqualTo(string.Empty),
            $"Expected Name to default to empty string but got: '{entity.Name}'");
        Assert.That(entity.Description, Is.EqualTo(string.Empty),
            $"Expected Description to default to empty string but got: '{entity.Description}'");
        Assert.That(entity.StorageKey, Is.EqualTo(string.Empty),
            $"Expected StorageKey to default to empty string but got: '{entity.StorageKey}'");
    }

    [Test]
    [DisplayName("Should set and get all properties correctly")]
    public void ShouldSetAndGetAllPropertiesCorrectly()
    {
        // Given: an entity with all properties assigned
        string expectedId = "recording-abc-123";
        string expectedName = "Sesion de meditacion matutina";
        string expectedDescription = "Una sesion guiada de meditacion para empezar el dia";
        RecordingType expectedType = RecordingType.Meditacion;
        string expectedStorageKey = "recordings/recording-abc-123/meditacion.mp4";
        string expectedCreatedBy = "admin-user-id";
        DateTimeOffset expectedCreated = new DateTimeOffset(2024, 1, 15, 10, 0, 0, TimeSpan.Zero);

        // When: all properties are set on the entity
        RecordingEntity entity = new()
        {
            Id = expectedId,
            Name = expectedName,
            Description = expectedDescription,
            Type = expectedType,
            StorageKey = expectedStorageKey,
            CreatedBy = expectedCreatedBy,
            Created = expectedCreated
        };

        // Then: all getters should return the assigned values
        Assert.That(entity.Id, Is.EqualTo(expectedId),
            $"Expected Id='{expectedId}' but got: '{entity.Id}'");
        Assert.That(entity.Name, Is.EqualTo(expectedName),
            $"Expected Name='{expectedName}' but got: '{entity.Name}'");
        Assert.That(entity.Description, Is.EqualTo(expectedDescription),
            $"Expected Description='{expectedDescription}' but got: '{entity.Description}'");
        Assert.That(entity.Type, Is.EqualTo(expectedType),
            $"Expected Type='{expectedType}' but got: '{entity.Type}'");
        Assert.That(entity.StorageKey, Is.EqualTo(expectedStorageKey),
            $"Expected StorageKey='{expectedStorageKey}' but got: '{entity.StorageKey}'");
        Assert.That(entity.CreatedBy, Is.EqualTo(expectedCreatedBy),
            $"Expected CreatedBy='{expectedCreatedBy}' but got: '{entity.CreatedBy}'");
        Assert.That(entity.Created, Is.EqualTo(expectedCreated),
            $"Expected Created='{expectedCreated}' but got: '{entity.Created}'");
    }
}

