using System.ComponentModel;
using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.DynamoRepositoryTest;

[TestFixture]
public class FindByIDAsyncTest : GenericDynamoRepositoryTest
{
    [Test]
    [DisplayName("Should return entity when ID exists in DynamoDB")]
    public async Task ShouldReturnEntityWhenIdExists()
    {
        // Given: A valid ID and an entity in DynamoDB
        const string id = "test-id";
        TestEntity entity = new() { ID = id };
        _contextMock.Setup(x => x.LoadAsync<TestEntity>(id, CancellationToken.None))
            .ReturnsAsync(entity);

        // When: Finding by ID
        Result<TestEntity> result = await _repository.ExposedFindByID(id);

        // Then: Result should be success and contain the entity
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(entity));
        _contextMock.Verify(x => x.LoadAsync<TestEntity>(id, CancellationToken.None), Times.Once);
    }

    [Test]
    [DisplayName("Should return failure result when DynamoDB throws exception on Load")]
    public async Task ShouldReturnFailureWhenLoadThrowsException()
    {
        // Given: An exception in the context
        _contextMock.Setup(x => x.LoadAsync<TestEntity>(It.IsAny<string>(), CancellationToken.None))
            .ThrowsAsync(new Exception("DB Connection Error"));

        // When: Finding by ID
        Result<TestEntity> result = await _repository.ExposedFindByID("any");

        // Then: Should return failure with the handled error message
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.GeneralError));
    }

}
