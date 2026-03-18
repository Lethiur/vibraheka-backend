using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.ActionLogRepositoryTest;

[TestFixture]
public class SaveActionLogTest : GenericActionLogRepositoryIntegrationTest
{
    [Test]
    public async Task ShouldSaveActionLogSuccessfully()
    {
        // Given: un action log valido.
        ActionLogEntity actionLog = new()
        {
            ID = Guid.NewGuid().ToString(),
            Action = ActionType.UserVerification,
            Timestamp = DateTimeOffset.UtcNow
        };

        // When: se guarda el action log.
        Result<ActionLogEntity> result = await _repository.SaveActionLog(actionLog, CancellationToken.None);

        // Then: debe guardarse exitosamente.
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.ID, Is.EqualTo(actionLog.ID));
    }

    [Test]
    public async Task ShouldReturnGeneralErrorWhenSaveOperationIsCancelled()
    {
        // Given: un action log valido y token de cancelacion cancelado.
        ActionLogEntity actionLog = new()
        {
            ID = Guid.NewGuid().ToString(),
            Action = ActionType.UserVerification,
            Timestamp = DateTimeOffset.UtcNow
        };
        using CancellationTokenSource cts = new();
        cts.Cancel();

        // When: se intenta guardar con operacion cancelada.
        Result<ActionLogEntity> result = await _repository.SaveActionLog(actionLog, cts.Token);

        // Then: debe retornar error general de persistencia.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.GeneralError));
    }
}
