using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Entities;
using Moq;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.SubscriptionRepositoryTest;

[TestFixture]
public class DeleteSubscriptionForUserTest : GenericSubscriptionRepositoryTest
{
    [Test]
    public async Task ShouldReturnSuccessWhenDeleteSucceeds()
    {
        // Given
        SubscriptionEntity entity = new() { SubscriptionID = "sub-1", UserID = "user-1" };

        ContextMock.Setup(x => x.DeleteAsync(It.IsAny<object>(), It.IsAny<Amazon.DynamoDBv2.DataModel.DeleteConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // When
        Result<Unit> result = await Repository.DeleteSubscriptionForUser(entity, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(Unit.Value));
    }
}
