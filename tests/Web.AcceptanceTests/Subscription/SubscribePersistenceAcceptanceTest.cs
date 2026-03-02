using System.Net;
using CSharpFunctionalExtensions;
using NUnit.Framework;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Common.Interfaces.Orders;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Entities;

namespace VibraHeka.Web.AcceptanceTests.Subscription;

[TestFixture]
public class SubscribePersistenceAcceptanceTest : GenericSubscriptionAcceptanceTest
{
    [Test]
    public async Task ShouldPersistSubscriptionInDynamoDbWithExpectedFieldsWhenUserSubscribes()
    {
        // Given: a confirmed and authenticated user.
        Domain.Models.Results.AuthenticationResult authResult = await AuthenticateAsConfirmedUser();
        IUserRepository userRepository = GetObjectFromFactory<IUserRepository>();
        StripeConfig stripeConfig = GetObjectFromFactory<StripeConfig>();

        Result<UserEntity> userBeforeSubscriptionResult =
            await userRepository.GetByIdAsync(authResult.UserID, CancellationToken.None);
        Assert.That(userBeforeSubscriptionResult.IsSuccess, Is.True);
        UserEntity userBeforeSubscription = userBeforeSubscriptionResult.Value;

        // When: the user starts the subscription flow.
        HttpResponseMessage subscribeResponse = await Client.PutAsync("/api/v1/subscriptions", null);

        // Then: the API call should succeed.
        Assert.That(subscribeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // And: the subscription persisted in DynamoDB should match expected values.
        ISubscriptionRepository subscriptionRepository = GetObjectFromFactory<ISubscriptionRepository>();
        Result<SubscriptionEntity> subscriptionResult =
            await subscriptionRepository.GetSubscriptionDetailsForUser(authResult.UserID, CancellationToken.None);

        Assert.That(subscriptionResult.IsSuccess, Is.True);
        SubscriptionEntity subscription = subscriptionResult.Value;

        Result<UserEntity> userAfterSubscriptionResult =
            await userRepository.GetByIdAsync(authResult.UserID, CancellationToken.None);
        Assert.That(userAfterSubscriptionResult.IsSuccess, Is.True);
        UserEntity userAfterSubscription = userAfterSubscriptionResult.Value;

        Assert.That(subscription.UserID, Is.EqualTo(authResult.UserID));
        Assert.That(subscription.ExternalCustomerID, Is.EqualTo(userAfterSubscription.CustomerID));
        Assert.That(subscription.ExternalCustomerID, Is.Not.Empty);
        Assert.That(subscription.ExternalSubscriptionItemID, Is.EqualTo(stripeConfig.SubscriptionID));
        Assert.That(subscription.ExternalSubscriptionID, Is.Empty);
        Assert.That(subscription.CheckoutSessionUrl, Is.Not.Empty);
        Assert.That(subscription.Status, Is.EqualTo(OrderStatus.Pending));
        Assert.That(subscription.SubscriptionStatus, Is.EqualTo(SubscriptionStatus.Created));

        Assert.That(userBeforeSubscription.CustomerID, Is.Empty);
        Assert.That(userAfterSubscription.CustomerID, Is.EqualTo(userAfterSubscription.CustomerID));
    }
}
