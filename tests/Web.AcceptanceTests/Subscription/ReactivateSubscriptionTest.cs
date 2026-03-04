using System.Net;
using NUnit.Framework;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.Subscription;

[TestFixture]
public class ReactivateSubscriptionTest : GenericSubscriptionAcceptanceTest
{
    [Test]
    public async Task ShouldReturnUnauthorizedWhenNotAuthenticated()
    {
        // Given: no authentication token is attached to the request.

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

        ResponseEntity entity = await response.GetAsResponseEntity();
        Assert.That(entity.Success, Is.False);
        Assert.That(entity.ErrorCode, Is.EqualTo(SubscriptionErrors.NoSubscriptionFound));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenSubscriptionIsNotMarkedAsToBeCancelled()
    {
        // Given: an authenticated user with a subscription already active (first ensure branch).
        Domain.Models.Results.AuthenticationResult authResult = await AuthenticateAsConfirmedUser();
        var seedResult = await SeedSubscriptionForUser(authResult.UserID, SubscriptionStatus.Active, OrderStatus.Pending);
        Assert.That(seedResult.IsSuccess, Is.True);

        // When: requesting reactivation.
        HttpResponseMessage response = await Client.PatchAsync("/api/v1/subscriptions/reactivate", null);

        // Then: the service blocks reactivation as already active.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        ResponseEntity entity = await response.GetAsResponseEntity();
        Assert.That(entity.Success, Is.False);
        Assert.That(entity.ErrorCode, Is.EqualTo(SubscriptionErrors.SubscriptionIsActive));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenSubscriptionOrderStatusIsPaymentFailed()
    {
        // Given: an authenticated user with ToBeCancelled + PaymentFailed (second ensure branch).
        Domain.Models.Results.AuthenticationResult authResult = await AuthenticateAsConfirmedUser();
        var seedResult = await SeedSubscriptionForUser(authResult.UserID, SubscriptionStatus.ToBeCancelled,
            OrderStatus.PaymentFailed);
        Assert.That(seedResult.IsSuccess, Is.True);

        // When: requesting reactivation.
        HttpResponseMessage response = await Client.PatchAsync("/api/v1/subscriptions/reactivate", null);

        // Then: the service maps it as cancelled and rejects reactivation.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        ResponseEntity entity = await response.GetAsResponseEntity();
        Assert.That(entity.Success, Is.False);
        Assert.That(entity.ErrorCode, Is.EqualTo(SubscriptionErrors.SubscriptionIsCancelled));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenSubscriptionOrderStatusIsCancelled()
    {
        // Given: an authenticated user with ToBeCancelled + Cancelled (second ensure alternate branch).
        Domain.Models.Results.AuthenticationResult authResult = await AuthenticateAsConfirmedUser();
        var seedResult = await SeedSubscriptionForUser(authResult.UserID, SubscriptionStatus.ToBeCancelled,
            OrderStatus.Cancelled);
        Assert.That(seedResult.IsSuccess, Is.True);

        // When: requesting reactivation.
        HttpResponseMessage response = await Client.PatchAsync("/api/v1/subscriptions/reactivate", null);

        // Then: the service returns SubscriptionIsCancelled.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        ResponseEntity entity = await response.GetAsResponseEntity();
        Assert.That(entity.Success, Is.False);
        Assert.That(entity.ErrorCode, Is.EqualTo(SubscriptionErrors.SubscriptionIsCancelled));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenStripeReactivationFailsAfterEnsureChecks()
    {
        // Given: a subscription that passes ensure checks but has fake external id that Stripe will reject.
        Domain.Models.Results.AuthenticationResult authResult = await AuthenticateAsConfirmedUser();
        var seedResult = await SeedSubscriptionForUser(authResult.UserID, SubscriptionStatus.ToBeCancelled,
            OrderStatus.Pending);
        Assert.That(seedResult.IsSuccess, Is.True);

        // When: attempting to reactivate subscription.
        HttpResponseMessage response = await Client.PatchAsync("/api/v1/subscriptions/reactivate", null);

        // Then: operation fails with stripe infrastructure mapping.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        ResponseEntity entity = await response.GetAsResponseEntity();
        Assert.That(entity.Success, Is.False);
        Assert.That(entity.ErrorCode, Is.EqualTo(InfrastructureSubscriptionErrors.StripeError));
    }
}
