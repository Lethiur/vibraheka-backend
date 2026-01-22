using System.ComponentModel;
using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using static System.Threading.CancellationToken;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.DynamoRepositoryTest;

[TestFixture]
public class SaveAsyncTest : GenericDynamoRepositoryTest
{
    [Test]
    [DisplayName("Should return success Unit when entity is saved successfully")]
    public async Task ShouldReturnSuccessUnitWhenSaveIsSuccessful()
    {
        // Given: An entity to save
        TestEntity entity = new TestEntity { ID = "save-id" };
        _contextMock.Setup(x => x.SaveAsync(entity, It.IsAny<SaveConfig>(), None))
            .Returns(Task.CompletedTask);

        // When: Saving the entity
        Result<Unit> result = await _repository.ExposedSave(entity);

        // Then: Result should be success Unit
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(Unit.Value));
        _contextMock.Verify(x => x.SaveAsync(entity, 
            It.Is<SaveConfig>(c => c.OverrideTableName == TableName), None), Times.Once);
    }

    [Test]
    [DisplayName("Should return failure result when DynamoDB throws exception on Save")]
    public async Task ShouldReturnFailureWhenSaveThrowsException()
    {
        // Given: A database error during save
        TestEntity entity = new TestEntity { ID = "fail-id" };
        _contextMock.Setup(x => x.SaveAsync(entity, It.IsAny<SaveConfig>(), None))
            .ThrowsAsync(new Exception("Write Error"));

        // When: Saving the entity
        Result<Unit> result = await _repository.ExposedSave(entity);

        // Then: Should return failure with the handled error message
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("Handled: Write Error"));
    }
}
