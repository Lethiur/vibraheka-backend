using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.SubscriptionRepositoryTest;

[TestFixture]
public class DeleteSubscriptionForUserTest : GenericSubscriptionRepositoryIntegrationTest
{
    [Test]
    public async Task ShouldDeleteSubscriptionWhenItExists()
    {
        // Given: una suscripcion persistida para el usuario.
        string userId = Guid.NewGuid().ToString();
        SubscriptionEntity entity = new()
        {
            SubscriptionID = Guid.NewGuid().ToString(),
            UserID = userId,
            ExternalSubscriptionItemID = _stripeConfig.SubscriptionID,
            ExternalCustomerID = "cus_test_" + Guid.NewGuid().ToString("N"),
            SubscriptionStatus = SubscriptionStatus.Created,
            Status = OrderStatus.Pending,
            Created = DateTime.UtcNow,
            CreatedBy = "integration-test"
        };

        await _repository.SaveSubscriptionAsync(entity, CancellationToken.None);

        // When: se elimina la suscripcion y luego se consulta.
        Result<Unit> deleteResult = await _repository.DeleteSubscriptionForUser(entity, CancellationToken.None);
        Result<SubscriptionEntity> getResult = await _repository.GetSubscriptionDetailsForUser(userId, CancellationToken.None);

        // Then: la eliminacion debe ser exitosa y ya no debe existir.
        Assert.That(deleteResult.IsSuccess, Is.True);
        Assert.That(getResult.IsFailure, Is.True);
    }

    [Test]
    public async Task ShouldReturnGeneralErrorWhenDeleteOperationIsCancelled()
    {
        // Given: una suscripcion valida y un token cancelado.
        SubscriptionEntity entity = new()
        {
            SubscriptionID = Guid.NewGuid().ToString(),
            UserID = Guid.NewGuid().ToString(),
            ExternalSubscriptionItemID = _stripeConfig.SubscriptionID,
            ExternalCustomerID = "cus_test_" + Guid.NewGuid().ToString("N"),
            SubscriptionStatus = SubscriptionStatus.Created,
            Status = OrderStatus.Pending,
            Created = DateTime.UtcNow,
            CreatedBy = "integration-test"
        };
        using CancellationTokenSource cts = new();
        cts.Cancel();

        // When: se intenta eliminar con operacion cancelada.
        Result<Unit> deleteResult = await _repository.DeleteSubscriptionForUser(entity, cts.Token);

        // Then: debe devolverse error general de persistencia (catch de Delete).
        Assert.That(deleteResult.IsFailure, Is.True);
        Assert.That(deleteResult.Error, Is.EqualTo(GenericPersistenceErrors.GeneralError));
    }
}
