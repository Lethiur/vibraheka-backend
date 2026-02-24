using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.ActionLogRepositoryTest;

[TestFixture]
public class GetActionLogForUserTest : GenericActionLogRepositoryIntegrationTest
{
    [Test]
    public async Task ShouldGetActionLogForUserWhenItExists()
    {
        // Given
        string userId = Guid.NewGuid().ToString();
        ActionLogEntity actionLog = new()
        {
            ID = userId,
            Action = ActionType.RequestVerificationCode,
            Timestamp = DateTimeOffset.UtcNow
        };

        await _repository.SaveActionLog(actionLog, CancellationToken.None);

        // When
        Result<ActionLogEntity> result = await _repository.GetActionLogForUser(userId, ActionType.RequestVerificationCode, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.ID, Is.EqualTo(userId));
        Assert.That(result.Value.Action, Is.EqualTo(ActionType.RequestVerificationCode));
    }
}
