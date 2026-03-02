using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Internal.Entities;
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using Moq;
namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.SubscriptionRepositoryTest;

[TestFixture]
public class SaveSubscriptionAsyncTest : GenericSubscriptionRepositoryTest
{
    [Test]
    public async Task ShouldReturnInputSubscriptionWhenSaveSucceeds()
    {
        // Given
        AWSXRayRecorder.Instance.TraceContext.SetEntity(new Segment("mock"));
        SubscriptionEntity entity = new()
        {
            SubscriptionID = "sub-1",
            UserID = "user-1",
            SubscriptionStatus = SubscriptionStatus.Created
        };

        ContextMock.Setup(x => x.SaveAsync(It.IsAny<object>(), It.IsAny<Amazon.DynamoDBv2.DataModel.SaveConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // When
        Result<SubscriptionEntity> result = await Repository.SaveSubscriptionAsync(entity, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.SubscriptionID, Is.EqualTo("sub-1"));
    }
}
