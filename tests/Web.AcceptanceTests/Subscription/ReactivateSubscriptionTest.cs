using System.Net;
using NUnit.Framework;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Web.AcceptanceTests.Utils;
using VibraHeka.Web.Subscriptions;
using OrderStatus = VibraHeka.Domain.Commerce.Enums.OrderStatus;
using SubscriptionStatus = VibraHeka.Domain.Common.Enums.SubscriptionStatus;

namespace VibraHeka.Web.AcceptanceTests.Subscription;

[TestFixture]
public class ReactivateSubscriptionTest : GenericSubscriptionAcceptanceTest
{
    [Test]
    public async Task ShouldReturnUnauthorizedWhenNotAuthenticated()
    {
        // Given: no authentication token is attached to the request.
        Client.DefaultRequestHeaders.Remove("Authorization");

        // When: calling the reactivate subscription endpoint.
        HttpResponseMessage response = await Client.PatchAsync("/api/v1/subscriptions/reactivate", null);

        // Then: the endpoint rejects the call as unauthorized.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenUserHasNoSubscription()
    {
        // Given: an authenticated user without any persisted subscription.
        await AuthenticateAsConfirmedUser();

        // When: attempting to reactivate a subscription.
        HttpResponseMessage response = await Client.PatchAsync("/api/v1/subscriptions/reactivate", null);

        // Then: the operation fails with no-subscription error mapping.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        BadRequestResponse entity = await response.ParseContentAsync<BadRequestResponse>();
        Assert.That(entity.ErrorCode, Is.EqualTo(SubscriptionErrors.NoSubscriptionFound));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenSubscriptionIsNotMarkedAsToBeCancelled()
    {
        // Given: an authenticated user with a subscription already active (first ensure branch).
        await AuthenticateAsConfirmedUser();
        await Client.PutAsync("/api/v1/subscriptions", null);

        // When: requesting reactivation.
        HttpResponseMessage response = await Client.PatchAsync("/api/v1/subscriptions/reactivate", null);

        // Then: the service blocks reactivation as already active.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        BadRequestResponse entity = await response.ParseContentAsync<BadRequestResponse>();
        Assert.That(entity.ErrorCode, Is.EqualTo(SubscriptionErrors.SubscriptionIsActive));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenSubscriptionOrderStatusIsPaymentFailed()
    {
        // Given: an authenticated user with ToBeCancelled + PaymentFailed (second ensure branch).
        await AuthenticateAsConfirmedUser();
        await Client.PutAsync("/api/v1/subscriptions", null);

        // When: requesting reactivation.
        HttpResponseMessage response = await Client.PatchAsync("/api/v1/subscriptions/reactivate", null);

        // Then: the service maps it as canceled and rejects reactivation.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        BadRequestResponse entity = await response.ParseContentAsync<BadRequestResponse>();
        Assert.That(entity.ErrorCode, Is.EqualTo(SubscriptionErrors.SubscriptionIsCancelled));
    }
    
    [Test]
    public async Task ShouldReturnBadRequestWhenStripeReactivationFailsAfterEnsureChecks()
    {
        // Given: a subscription that passes ensure checks but has fake external id that Stripe will reject.
        await AuthenticateAsConfirmedUser();
        await Client.PutAsync("/api/v1/subscriptions", null);
        
        // When: attempting to reactivate subscription.
        HttpResponseMessage response = await Client.PatchAsync("/api/v1/subscriptions/reactivate", null);

        // Then: operation fails with stripe infrastructure mapping.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        BadRequestResponse entity = await response.ParseContentAsync<BadRequestResponse>();
        Assert.That(entity.ErrorCode, Is.EqualTo(InfrastructureSubscriptionErrors.StripeError));
    }
}
