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
        // Given

        // When
        HttpResponseMessage response = await Client.GetAsync("/api/v1/subscriptions/details");

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldReturnPortalUrlWhenAuthenticatedUserHasCustomer()
    {
        // Given
        await AuthenticateAsConfirmedUser();
        HttpResponseMessage subscribeResponse = await Client.PutAsync("/api/v1/subscriptions", null);
        Assert.That(subscribeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // When
        HttpResponseMessage response = await Client.GetAsync("/api/v1/subscriptions/details");

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        ResponseEntity entity = await response.GetAsResponseEntityAndContentAs<string>();
        string? portalUrl = entity.GetContentAs<string>();

        Assert.That(entity.Success, Is.True);
        Assert.That(portalUrl, Is.Not.Null.And.Not.Empty);
        Assert.That(portalUrl!.StartsWith("https://billing.stripe.com/"), Is.True);
    }
}
