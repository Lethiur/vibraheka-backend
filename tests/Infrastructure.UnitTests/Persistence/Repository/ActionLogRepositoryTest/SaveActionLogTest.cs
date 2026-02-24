using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Internal.Entities;
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using Moq;
namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.ActionLogRepositoryTest;

[TestFixture]
public class SaveActionLogTest : GenericActionLogRepositoryTest
{
    [Test]
    public async Task ShouldReturnInputActionLogWhenSaveSucceeds()
    {
        // Given
        AWSXRayRecorder.Instance.TraceContext.SetEntity(new Segment("mock"));
        ActionLogEntity entity = new()
        {
            ID = "user-1",
            Action = ActionType.UserVerification,
            Timestamp = DateTimeOffset.UtcNow
        };

        ContextMock.Setup(x => x.SaveAsync(It.IsAny<object>(), It.IsAny<Amazon.DynamoDBv2.DataModel.SaveConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // When
        Result<ActionLogEntity> result = await Repository.SaveActionLog(entity, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.ID, Is.EqualTo("user-1"));
    }
}
