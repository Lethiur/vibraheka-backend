using System.Net.Http.Headers;
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Common.Interfaces.Orders;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.Subscription;

public abstract class GenericSubscriptionAcceptanceTest : GenericAcceptanceTest<VibraHekaProgram>
{
    protected async Task<AuthenticationResult> AuthenticateAsConfirmedUser()
    {
        // Given
        string email = TheFaker.Internet.Email();
        string username = TheFaker.Person.FullName;
        await RegisterAndConfirmUser(username, email, ThePassword);

        // When
        AuthenticationResult authResult = await AuthenticateUser(email, ThePassword);

        // Then
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
        return authResult;
    }

    protected async Task<Result<SubscriptionEntity>> SeedSubscriptionForUser(
        string userId,
        SubscriptionStatus subscriptionStatus,
        OrderStatus orderStatus,
        DateTimeOffset? startDate = null)
    {
        // Given: a subscription entity prepared with a specific state combination.
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
            StartDate = startDate ?? DateTimeOffset.UtcNow.AddDays(-1),
            EndDate = DateTimeOffset.UtcNow.AddDays(30),
            Created = DateTime.UtcNow,
            CreatedBy = "acceptance-test",
            LastModified = DateTime.UtcNow,
            LastModifiedBy = "acceptance-test"
        };

        // When: saving it to the subscriptions table.
        Result<SubscriptionEntity> saveResult =
            await subscriptionRepository.SaveSubscriptionAsync(subscriptionEntity, CancellationToken.None);

        // Then: caller can assert whether persistence succeeded before invoking the API.
        return saveResult;
    }
}
