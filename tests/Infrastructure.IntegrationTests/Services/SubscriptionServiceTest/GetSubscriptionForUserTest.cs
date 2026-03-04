using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.SubscriptionServiceTest;

[TestFixture]
public class GetSubscriptionForUserTest : GenericSubscriptionServiceIntegrationTest
{
    [Test]
    public async Task ShouldGetSubscriptionForExistingUser()
    {
        // Given: un usuario con suscripcion persistida.
        string userId = Guid.NewGuid().ToString();
        await _subscriptionRepository.SaveSubscriptionAsync(new SubscriptionEntity
        {
            SubscriptionID = Guid.NewGuid().ToString(),
            UserID = userId,
            ExternalSubscriptionItemID = _stripeConfig.SubscriptionID,
            ExternalCustomerID = "cus_test_" + Guid.NewGuid().ToString("N"),
            SubscriptionStatus = SubscriptionStatus.Created,
            Status = OrderStatus.Pending,
            Created = DateTime.UtcNow,
            CreatedBy = "integration-test"
        }, CancellationToken.None);

        // When: se consulta la suscripcion por user id.
        Result<SubscriptionEntity> result = await _service.GetSubscriptionForUser(userId, CancellationToken.None);

        // Then: debe devolverse la suscripcion del usuario.
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.UserID, Is.EqualTo(userId));
    }

    [Test]
    public async Task ShouldFailWhenSubscriptionDoesNotExist()
    {
        // Given: un user id sin suscripcion asociada.
        string userIdWithoutSubscription = Guid.NewGuid().ToString();

        // When: se intenta recuperar una suscripcion inexistente.
        Result<SubscriptionEntity> result = await _service.GetSubscriptionForUser(userIdWithoutSubscription, CancellationToken.None);

        // Then: debe devolverse error de no suscripcion encontrada.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SubscriptionErrors.NoSubscriptionFound));
    }
}
