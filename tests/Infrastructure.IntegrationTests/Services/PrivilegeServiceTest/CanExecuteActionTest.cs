using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.PrivilegeServiceTest;

public class CanExecuteActionTest : GenericPrivilegeServiceTest
{
    [Test]
    public async Task ShouldReturnTrueIfTheActionDoesNotExist()
    {
        // Given: un user id valido sin registro previo para la accion.
        string userID = Guid.NewGuid().ToString();

        // When: se valida si puede ejecutar la accion.
        Result<bool> canExecuteAction = await PrivilegeService.CanExecuteAction(
            userID,
            ActionType.RequestVerificationCode,
            CancellationToken.None);

        // Then: debe devolver true y persistir un action log.
        Assert.That(canExecuteAction.IsSuccess, Is.True);
        Assert.That(canExecuteAction.Value, Is.True);

        Result<ActionLogEntity> actionLogForUser = await _actionLogRepository.GetActionLogForUser(
            userID,
            ActionType.RequestVerificationCode,
            CancellationToken.None);
        Assert.That(actionLogForUser.IsSuccess, Is.True);
    }

    [Test]
    public async Task ShouldFailIfTwoConcurrentRequestsHappen()
    {
        // Given: un action log reciente para la misma accion (en cooldown).
        ActionLogEntity actionLogEntity = new()
        {
            Action = ActionType.RequestVerificationCode,
            ID = Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow,
        };
        await _actionLogRepository.SaveActionLog(actionLogEntity, CancellationToken.None);

        // When: se intenta ejecutar la accion nuevamente.
        Result<bool> canExecuteAction = await PrivilegeService.CanExecuteAction(
            actionLogEntity.ID,
            ActionType.RequestVerificationCode,
            CancellationToken.None);

        // Then: debe devolver false por cooldown.
        Assert.That(canExecuteAction.IsFailure, Is.False);
        Assert.That(canExecuteAction.Value, Is.False);
    }

    [Test]
    public async Task ShouldReturnInvalidIdWhenUserIdIsNull()
    {
        // Given: un user id nulo.

        // When: se valida la accion con id nulo.
        Result<bool> result = await PrivilegeService.CanExecuteAction(
            null!,
            ActionType.PasswordReset,
            CancellationToken.None);

        // Then: debe fallar con InvalidID.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(PrivilegeErrors.InvalidID));
    }

    [Test]
    public async Task ShouldReturnGeneralPersistenceErrorWhenUserIdIsEmptyString()
    {
        // Given: un user id vacio.

        // When: se valida la accion con id vacio.
        Result<bool> result = await PrivilegeService.CanExecuteAction(
            "",
            ActionType.PasswordReset,
            CancellationToken.None);

        // Then: en la implementacion actual se propaga error de persistencia.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.GeneralError));
    }

    [Test]
    public async Task ShouldReturnGenericPersistenceErrorWhenReadOperationIsCancelled()
    {
        // Given: un token de cancelacion ya cancelado.
        using CancellationTokenSource cts = new();
        cts.Cancel();

        // When: se valida la accion con operacion cancelada.
        Result<bool> result = await PrivilegeService.CanExecuteAction(
            Guid.NewGuid().ToString(),
            ActionType.PasswordReset,
            cts.Token);

        // Then: debe devolver error general de persistencia.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.GeneralError));
    }
}
