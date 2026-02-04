using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using static VibraHeka.Domain.Exceptions.ActionLogErrors;

namespace VibraHeka.Infrastructure.UnitTests.Services.PrivilegeServiceTest;

[TestFixture]
public class CanExecuteActionTest : GenericPrivilegeServiceTest
{
    [Test]
    public async Task ShouldReturnTrueIEverythingIsGood()
    {
        // Given: Some nice ass mocking
        _actionLogRepositoryMock.Setup(repo => repo.GetActionLogForUser(It.IsAny<string>(), It.IsAny<ActionType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new ActionLogEntity {Timestamp = DateTime.UtcNow.AddDays(-1)}));
        
        _actionLogRepositoryMock.Setup(repo => repo.SaveActionLog(It.IsAny<ActionLogEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new ActionLogEntity {ID = "Testing"}));
        
        // When: The stuff is invoked
        Result<bool> canExecuteAction = await _service.CanExecuteAction("test",ActionType.PasswordReset, CancellationToken.None);
        
        // Then: Should return true
        Assert.That(canExecuteAction.IsSuccess, Is.True);
        Assert.That(canExecuteAction.Value, Is.True);
        
        // And: Mocks should have been invoked only once
        _actionLogRepositoryMock.Verify(repo => repo.GetActionLogForUser(It.IsAny<string>(), It.IsAny<ActionType>(), It.IsAny<CancellationToken>()), Times.Once);
        _actionLogRepositoryMock.Verify(repo => repo.SaveActionLog(It.IsAny<ActionLogEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ShouldReturnFalseWhenTheActionIsOnCooldown()
    {
        // Given: Some nice ass mocking
        _actionLogRepositoryMock.Setup(repo => repo.GetActionLogForUser(It.IsAny<string>(), It.IsAny<ActionType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new ActionLogEntity()));
        
        _actionLogRepositoryMock.Setup(repo => repo.SaveActionLog(It.IsAny<ActionLogEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new ActionLogEntity(){ID = "Testing"}));
        
        // When: The stuff is invoked
        Result<bool> canExecuteAction = await _service.CanExecuteAction("test",ActionType.PasswordReset, CancellationToken.None);
        
        // Then: Should return false
        Assert.That(canExecuteAction.IsSuccess, Is.True);
        Assert.That(canExecuteAction.Value, Is.False);
        
        // And: Mocks should have been invoked only once
        _actionLogRepositoryMock.Verify(repo => repo.GetActionLogForUser(It.IsAny<string>(), It.IsAny<ActionType>(), It.IsAny<CancellationToken>()), Times.Once);
        _actionLogRepositoryMock.Verify(repo => repo.SaveActionLog(It.IsAny<ActionLogEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ShouldHandleErrorFromReading()
    {
        // Given: Some nice ass mocking
        _actionLogRepositoryMock.Setup(repo => repo.GetActionLogForUser(It.IsAny<string>(), It.IsAny<ActionType>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));
        
        _actionLogRepositoryMock.Setup(repo => repo.SaveActionLog(It.IsAny<ActionLogEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new ActionLogEntity(){ID = "Testing"}));
        
        // When: The stuff is invoked
        Result<bool> canExecuteAction = await _service.CanExecuteAction("test",ActionType.PasswordReset, CancellationToken.None);
        
        // Then: Should return false
        Assert.That(canExecuteAction.IsSuccess, Is.False);
        Assert.That(canExecuteAction.Error, Is.EqualTo("Database error"));
        
        
        
        // And: Mocks should have been invoked only once
        _actionLogRepositoryMock.Verify(repo => repo.GetActionLogForUser(It.IsAny<string>(), It.IsAny<ActionType>(), It.IsAny<CancellationToken>()), Times.Once);
        _actionLogRepositoryMock.Verify(repo => repo.SaveActionLog(It.IsAny<ActionLogEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ShouldReturnErrorFromSaving()
    {
        // Given: Some nice ass mocking
        _actionLogRepositoryMock.Setup(repo => repo.GetActionLogForUser(It.IsAny<string>(), It.IsAny<ActionType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new ActionLogEntity(){Timestamp = DateTime.UtcNow.AddDays(-1)}));
        
        _actionLogRepositoryMock.Setup(repo => repo.SaveActionLog(It.IsAny<ActionLogEntity>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));
        
        // When: The stuff is invoked
        Result<bool> canExecuteAction = await _service.CanExecuteAction("test",ActionType.PasswordReset, CancellationToken.None);
        
        // Then: Should return false
        Assert.That(canExecuteAction.IsSuccess, Is.False);
        Assert.That(canExecuteAction.Error, Is.EqualTo("Database error"));
        
        // And: Mocks should have been invoked only once
        _actionLogRepositoryMock.Verify(repo => repo.GetActionLogForUser(It.IsAny<string>(), It.IsAny<ActionType>(), It.IsAny<CancellationToken>()), Times.Once);
        _actionLogRepositoryMock.Verify(repo => repo.SaveActionLog(It.IsAny<ActionLogEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ShouldInsertIfTheRecordDoesNotExist()
    {
        _actionLogRepositoryMock.Setup(repo => repo.GetActionLogForUser(It.IsAny<string>(), It.IsAny<ActionType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<ActionLogEntity>(ActionLogNotFound));
        
        _actionLogRepositoryMock.Setup(repo => repo.SaveActionLog(It.IsAny<ActionLogEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new ActionLogEntity(){ID = "Testing"}));
        
        // When: The stuff is invoked
        Result<bool> canExecuteAction = await _service.CanExecuteAction("test",ActionType.PasswordReset, CancellationToken.None);
        
        // Then: Should return true
        Assert.That(canExecuteAction.IsSuccess, Is.True);
        Assert.That(canExecuteAction.Value, Is.True);
        
        // And: Mocks should have been invoked only once
        _actionLogRepositoryMock.Verify(repo => repo.GetActionLogForUser(It.IsAny<string>(), It.IsAny<ActionType>(), It.IsAny<CancellationToken>()), Times.Once);
        _actionLogRepositoryMock.Verify(repo => repo.SaveActionLog(It.IsAny<ActionLogEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
