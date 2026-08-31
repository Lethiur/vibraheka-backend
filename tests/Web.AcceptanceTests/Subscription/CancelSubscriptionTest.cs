using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Exceptions;
using BadRequestResponse = VibraHeka.Web.Subscriptions.BadRequestResponse;

namespace VibraHeka.Web.AcceptanceTests.Subscription;

[TestFixture]
public class CancelSubscriptionTest : GenericSubscriptionAcceptanceTest
{
    [Test]
    public async Task ShouldReturnUnauthorizedWhenNotAuthenticated()
    {
        // Given: no authenticated user context.
        Client.DefaultRequestHeaders.Remove("Authorization");

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

        BadRequestResponse? entity = await response.Content.ReadFromJsonAsync<BadRequestResponse>();
        Assert.That(entity!.ErrorCode, Is.EqualTo(SubscriptionErrors.NoSubscriptionFound));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenStripeCancellationFailsForExistingSubscription()
    {
        // Given: an authenticated user with persisted subscription using a non-real external subscription id.
        await AuthenticateAsConfirmedUser();
        await Client.PutAsync("/api/v1/subscriptions", null);
        

        // When: requesting cancellation for that subscription.
        HttpResponseMessage response = await Client.PatchAsync("/api/v1/subscriptions", null);

        // Then: cancellation should fail with mapped stripe infrastructure error.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        BadRequestResponse? entity = await response.Content.ReadFromJsonAsync<BadRequestResponse>();
        Assert.That(entity!.ErrorCode, Is.EqualTo(InfrastructureSubscriptionErrors.StripeError));
    }
}
