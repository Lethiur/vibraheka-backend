using System.Net;
using NUnit.Framework;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.AcceptanceTests.Generic;
using VibraHeka.Web.Entities;

namespace VibraHeka.Web.AcceptanceTests.Subscription;

[TestFixture]
public class SubscribeTest : GenericSubscriptionAcceptanceTest
{
    [Test]
    public async Task ShouldReturnUnauthorizedWhenNotAuthenticated()
    {
        // Given

        // When
        HttpResponseMessage response = await Client.PutAsync("/api/v1/subscriptions", null);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldCreateSubscriptionAndReturnCheckoutUrlWhenAuthenticated()
    {
        // Given
        await AuthenticateAsConfirmedUser();

        // When
        HttpResponseMessage response = await Client.PutAsync("/api/v1/subscriptions", null);
        string responseBody = await response.Content.ReadAsStringAsync();

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            $"Expected OK but got {(int)response.StatusCode} ({response.StatusCode}). Body: {responseBody}");

        ResponseEntity entity = await response.GetAsResponseEntityAndContentAs<SubscriptionCreationDTO>();
        SubscriptionCreationDTO? checkoutUrl = entity.GetContentAs<SubscriptionCreationDTO>();

        Assert.That(entity.Success, Is.True);
        Assert.That(checkoutUrl!.Url.StartsWith("https://checkout.stripe.com/"), Is.True);
    }
}
