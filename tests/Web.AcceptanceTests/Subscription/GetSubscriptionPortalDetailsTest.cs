using System.Net;
using NUnit.Framework;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.Subscription;

[TestFixture]
public class GetSubscriptionPortalDetailsTest : GenericSubscriptionAcceptanceTest
{
    [Test]
    public async Task ShouldReturnUnauthorizedWhenNotAuthenticated()
    {
        // Given: no authentication token.

        // When: requesting billing portal details.
        HttpResponseMessage response = await Client.GetAsync("/api/v1/subscriptions/details");

        // Then: the endpoint should reject the request.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenAuthenticatedUserHasNoCustomerId()
    {
        // Given: an authenticated user that has not started a subscription yet.
        await AuthenticateAsConfirmedUser();

        // When: requesting billing portal url without customer id.
        HttpResponseMessage response = await Client.GetAsync("/api/v1/subscriptions/details");

        // Then: the endpoint should return a mapped bad request error.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        ResponseEntity entity = await response.GetAsResponseEntity();
        Assert.That(entity.Success, Is.False);
    }

    [Test]
    public async Task ShouldReturnPortalUrlWhenAuthenticatedUserHasCustomer()
    {
        // Given: an authenticated user with a customer in Stripe after starting subscription flow.
        await AuthenticateAsConfirmedUser();
        HttpResponseMessage subscribeResponse = await Client.PutAsync("/api/v1/subscriptions", null);
        Assert.That(subscribeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // When: requesting the billing portal url.
        HttpResponseMessage response = await Client.GetAsync("/api/v1/subscriptions/details");

        // Then: the endpoint should return a valid portal url.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        ResponseEntity entity = await response.GetAsResponseEntityAndContentAs<string>();
        string? portalUrl = entity.GetContentAs<string>();

        Assert.That(entity.Success, Is.True);
        Assert.That(portalUrl, Is.Not.Null.And.Not.Empty);
        Assert.That(portalUrl!.StartsWith("https://billing.stripe.com/"), Is.True);
    }
}
