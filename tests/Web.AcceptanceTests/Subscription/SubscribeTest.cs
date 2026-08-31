using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using VibraHeka.Web.Subscriptions;

namespace VibraHeka.Web.AcceptanceTests.Subscription;

[TestFixture]
public class SubscribeTest : GenericSubscriptionAcceptanceTest
{
    [Test]
    public async Task ShouldReturnUnauthorizedWhenNotAuthenticated()
    {
        // Given
        Client.DefaultRequestHeaders.Remove("Authorization");

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

        SubscriptionResponse? entity = await response.Content.ReadFromJsonAsync<SubscriptionResponse>();
        
        Assert.That(entity!.Url.StartsWith("https://checkout.stripe.com/"), Is.True);
    }
}
