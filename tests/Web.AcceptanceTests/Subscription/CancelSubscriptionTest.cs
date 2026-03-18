using System.Net;
using NUnit.Framework;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.Subscription;

[TestFixture]
public class CancelSubscriptionTest : GenericSubscriptionAcceptanceTest
{
    [Test]
    public async Task ShouldReturnUnauthorizedWhenNotAuthenticated()
    {
        // Given: no authenticated user context.

        // When: invoking subscription cancellation.
        HttpResponseMessage response = await Client.PatchAsync("/api/v1/subscriptions", null);

        // Then: endpoint should reject with unauthorized.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenUserHasNoSubscription()
    {
        // Given: an authenticated user without a persisted subscription.
        await AuthenticateAsConfirmedUser();

        // When: invoking subscription cancellation.
        HttpResponseMessage response = await Client.PatchAsync("/api/v1/subscriptions", null);

        // Then: service should map the missing subscription error.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        ResponseEntity entity = await response.GetAsResponseEntity();
        Assert.That(entity.Success, Is.False);
        Assert.That(entity.ErrorCode, Is.EqualTo(SubscriptionErrors.NoSubscriptionFound));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenStripeCancellationFailsForExistingSubscription()
    {
        // Given: an authenticated user with persisted subscription using a non-real external subscription id.
        Domain.Models.Results.AuthenticationResult authResult = await AuthenticateAsConfirmedUser();
        var saveResult = await SeedSubscriptionForUser(
            authResult.UserID,
            Domain.Common.Enums.SubscriptionStatus.Active,
            Domain.Common.Enums.OrderStatus.Pending);
        Assert.That(saveResult.IsSuccess, Is.True);

        // When: requesting cancellation for that subscription.
        HttpResponseMessage response = await Client.PatchAsync("/api/v1/subscriptions", null);

        // Then: cancellation should fail with mapped stripe infrastructure error.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        ResponseEntity entity = await response.GetAsResponseEntity();
        Assert.That(entity.Success, Is.False);
        Assert.That(entity.ErrorCode, Is.EqualTo(InfrastructureSubscriptionErrors.StripeError));
    }
}
