using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.ActionLogRepositoryTest;

[TestFixture]
public class SaveActionLogTest : GenericActionLogRepositoryIntegrationTest
{
    [Test]
    public async Task ShouldSaveActionLogSuccessfully()
    {
        // Given
        ActionLogEntity actionLog = new()
        {
            ID = Guid.NewGuid().ToString(),
            Action = ActionType.UserVerification,
            Timestamp = DateTimeOffset.UtcNow
        };

        // When
        Result<ActionLogEntity> result = await _repository.SaveActionLog(actionLog, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.ID, Is.EqualTo(actionLog.ID));
    }
}
