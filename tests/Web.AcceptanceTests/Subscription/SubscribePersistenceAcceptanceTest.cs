using System.Net;
using CSharpFunctionalExtensions;
using Infrastructure.AWS.DynamoDB.Subscriptions.Adapters;
using Infrastructure.AWS.DynamoDB.Users.Adapters;
using NUnit.Framework;
using VibraHeka.Domain.Common.Enums;
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
        UserProfileAdapter userRepository = GetObjectFromFactory<UserProfileAdapter>();
        StripeConfig stripeConfig = GetObjectFromFactory<StripeConfig>();

        Result<UserProfileEntity> userBeforeSubscriptionResult =
            await userRepository.GetProfileByUserId(authResult.UserID, CancellationToken.None);
        Assert.That(userBeforeSubscriptionResult.IsSuccess, Is.True);
        UserProfileEntity userProfileBeforeSubscription = userBeforeSubscriptionResult.Value;

        // When: the user starts the subscription flow.
        HttpResponseMessage subscribeResponse = await Client.PutAsync("/api/v1/subscriptions", null);

        // Then: the API call should succeed.
        Assert.That(subscribeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // And: the subscription persisted in DynamoDB should match expected values.
        SubscriptionAdapter subscriptionRepository = GetObjectFromFactory<SubscriptionAdapter>();
        Result<SubscriptionEntity> subscriptionResult =
            await subscriptionRepository.GetSubscriptionForUser(authResult.UserID, CancellationToken.None);

        Assert.That(subscriptionResult.IsSuccess, Is.True);
        SubscriptionEntity subscription = subscriptionResult.Value;

        Result<UserProfileEntity> userAfterSubscriptionResult =
            await userRepository.GetProfileByUserId(authResult.UserID, CancellationToken.None);
        Assert.That(userAfterSubscriptionResult.IsSuccess, Is.True);
        UserProfileEntity userProfileAfterSubscription = userAfterSubscriptionResult.Value;

        Assert.That(subscription.UserID, Is.EqualTo(authResult.UserID));
        Assert.That(subscription.ExternalCustomerID, Is.EqualTo(userProfileAfterSubscription.CustomerID));
        Assert.That(subscription.ExternalCustomerID, Is.Not.Empty);
        Assert.That(subscription.ExternalSubscriptionItemID, Is.EqualTo(stripeConfig.SubscriptionID));
        Assert.That(subscription.ExternalSubscriptionID, Is.Empty);
        Assert.That(subscription.CheckoutSessionUrl, Is.Not.Empty);
        Assert.That(subscription.Status, Is.EqualTo(OrderStatus.Pending));
        Assert.That(subscription.SubscriptionStatus, Is.EqualTo(SubscriptionStatus.Created));

        Assert.That(userProfileBeforeSubscription.CustomerID, Is.Empty);
        Assert.That(userProfileAfterSubscription.CustomerID, Is.EqualTo(userProfileAfterSubscription.CustomerID));
    }
}
