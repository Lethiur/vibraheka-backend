using System.Net;
using NUnit.Framework;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.Entities;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.Subscription;

[TestFixture]
public class GetSubscriptionDetailsTest : GenericSubscriptionAcceptanceTest
{
    [Test]
    public async Task ShouldReturnUnauthorizedWhenNotAuthenticated()
    {
        // Given

        // When
        HttpResponseMessage response = await Client.GetAsync("/api/v1/subscriptions");

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldReturnSubscriptionDetailsWhenSubscriptionExists()
    {
        // Given
        await AuthenticateAsConfirmedUser();
        HttpResponseMessage subscribeResponse = await Client.PutAsync("/api/v1/subscriptions", null);
        Assert.That(subscribeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // When
        HttpResponseMessage response = await Client.GetAsync("/api/v1/subscriptions");

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        ResponseEntity entity = await response.GetAsResponseEntityAndContentAs<SubscriptionDetailsDTO>();
        SubscriptionDetailsDTO? details = entity.GetContentAs<SubscriptionDetailsDTO>();

        Assert.That(entity.Success, Is.True);
        Assert.That(details, Is.Not.Null);
    }
}
