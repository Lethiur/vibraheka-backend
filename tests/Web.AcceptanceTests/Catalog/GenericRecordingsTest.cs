using CSharpFunctionalExtensions;
using NMoneys;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Domain.Commerce.Enums;
using VibraHeka.Domain.Common.Interfaces.Subscription;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Web.AcceptanceTests.Generic;
using VibraHeka.Web.Entities;
using SubscriptionStatus = VibraHeka.Domain.Common.Enums.SubscriptionStatus;

namespace VibraHeka.Web.AcceptanceTests.Catalog;

/// <summary>
/// Helpers para construir cuerpos de petición JSON de grabaciones.
/// Para autenticación y registro usar los métodos heredados de <see cref="Generic.GenericAcceptanceTest{TAppClass}"/>.
/// </summary>
public abstract class GenericRecordingsTest : GenericAcceptanceTest<VibraHekaProgram>
{
    protected UploadRecordingRequest BuildValidBody() => BuildBody();

    protected UploadRecordingRequest BuildPremiumBody() => BuildBody(tier: RecordingTier.Premium);

    protected UploadRecordingRequest BuildBody(
        string name = "Sesion de meditacion",
        string description = "Descripcion valida de la sesion de meditacion guiada",
        RecordingType type = RecordingType.Meditacion,
        RecordingTier tier = RecordingTier.Free)
    {
        UploadRecordingRequest request = new()
        {
            Name = name,
            Description = description,
            Type = type,
            Tier = tier,
            Price = 15m,
            CurrencyCode = CurrencyIsoCode.EUR
        };

        return request;
    }

    /// <summary>
    /// Seeds a subscription entity for a user directly in the repository, bypassing the Stripe payment flow.
    /// Intended exclusively for acceptance test setup in recording-access scenarios.
    /// </summary>
    protected async Task<Result<SubscriptionEntity>> SeedSubscriptionForRecordingTest(
        string userId,
        SubscriptionStatus subscriptionStatus,
        OrderStatus orderStatus)
    {
        ISubscriptionRepository subscriptionRepository = GetObjectFromFactory<ISubscriptionRepository>();
        StripeConfig stripeConfig = GetObjectFromFactory<StripeConfig>();

        SubscriptionEntity subscriptionEntity = new()
        {
            SubscriptionID = Guid.NewGuid().ToString(),
            UserID = userId,
            ExternalSubscriptionID = $"sub_test_{Guid.NewGuid():N}",
            ExternalSubscriptionItemID = stripeConfig.SubscriptionID,
            ExternalCustomerID = $"cus_test_{Guid.NewGuid():N}",
            CheckoutSessionUrl = "https://checkout.stripe.com/test_session",
            CheckoutSessionExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
            SubscriptionStatus = subscriptionStatus,
            Status = orderStatus,
            StartDate = DateTimeOffset.UtcNow.AddDays(-1),
            EndDate = DateTimeOffset.UtcNow.AddDays(30),
            Created = DateTime.UtcNow,
            CreatedBy = "acceptance-test",
            LastModified = DateTime.UtcNow,
            LastModifiedBy = "acceptance-test",
        };

        return await subscriptionRepository.SaveSubscriptionAsync(subscriptionEntity, CancellationToken.None);
    }
}
