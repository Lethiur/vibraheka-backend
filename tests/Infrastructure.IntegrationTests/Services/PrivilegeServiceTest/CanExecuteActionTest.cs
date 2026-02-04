using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.PrivilegeServiceTest;

public class CanExecuteActionTest : GenericPrivilegeServiceTest
{
    [Test]
    public async Task ShouldReturnTrueIfTheActionDoesNotExist()
    {
        // Given: a valid user id
        string userID = Guid.NewGuid().ToString();
        
        // When: The check is performed
        Result<bool> canExecuteAction = await PrivilegeService.CanExecuteAction(userID, ActionType.RequestVerificationCode, CancellationToken.None);
        
        // Then: Should return true
        Assert.That(canExecuteAction.IsSuccess, Is.True);
        
        // And: record should be in the DB
        Result<ActionLogEntity> actionLogForUser = await _actionLogRepository.GetActionLogForUser(userID, ActionType.RequestVerificationCode, CancellationToken.None);
        Assert.That(actionLogForUser.IsSuccess, Is.True);
    }

    [Test]
    public async Task ShouldFailIfTwoConcurrentRequestsHappen()
    {
        // Given: An entity in the DB
        ActionLogEntity actionLogEntity = new()
        {
            Action = ActionType.RequestVerificationCode,
            ID = Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow,
        };
        await _actionLogRepository.SaveActionLog(actionLogEntity, CancellationToken.None);
        
        // When: The action is requested
        Result<bool> canExecuteAction = await PrivilegeService.CanExecuteAction(actionLogEntity.ID, ActionType.RequestVerificationCode, CancellationToken.None);
        
        // Then: Should return false
        Assert.That(canExecuteAction.IsFailure, Is.False);
        Assert.That(canExecuteAction.Value, Is.False);
    }
}
