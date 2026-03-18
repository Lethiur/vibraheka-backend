using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.ActionLogRepositoryTest;

[TestFixture]
public class GetActionLogForUserTest : GenericActionLogRepositoryIntegrationTest
{
    [Test]
    public async Task ShouldGetActionLogForUserWhenItExists()
    {
        // Given: un action log existente para usuario y accion.
        string userId = Guid.NewGuid().ToString();
        ActionLogEntity actionLog = new()
        {
            ID = userId,
            Action = ActionType.RequestVerificationCode,
            Timestamp = DateTimeOffset.UtcNow
        };

        await _repository.SaveActionLog(actionLog, CancellationToken.None);

        // When: se consulta el action log por user id y accion.
        Result<ActionLogEntity> result = await _repository.GetActionLogForUser(
            userId,
            ActionType.RequestVerificationCode,
            CancellationToken.None);

        // Then: debe retornarse el registro guardado.
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.ID, Is.EqualTo(userId));
        Assert.That(result.Value.Action, Is.EqualTo(ActionType.RequestVerificationCode));
    }

    [Test]
    public async Task ShouldReturnActionLogNotFoundWhenRecordDoesNotExist()
    {
        // Given: un user id sin action log para la accion solicitada.
        string userId = Guid.NewGuid().ToString();

        // When: se consulta un registro inexistente.
        Result<ActionLogEntity> result = await _repository.GetActionLogForUser(
            userId,
            ActionType.RequestVerificationCode,
            CancellationToken.None);

        // Then: debe devolverse ActionLogNotFound.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(ActionLogErrors.ActionLogNotFound));
    }

    [Test]
    public async Task ShouldReturnGeneralErrorWhenOperationIsCancelled()
    {
        // Given: un cancellation token cancelado.
        using CancellationTokenSource cts = new();
        cts.Cancel();

        // When: se intenta consultar con operacion cancelada.
        Result<ActionLogEntity> result = await _repository.GetActionLogForUser(
            Guid.NewGuid().ToString(),
            ActionType.RequestVerificationCode,
            cts.Token);

        // Then: debe devolverse error general de persistencia.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.GeneralError));
    }
}
