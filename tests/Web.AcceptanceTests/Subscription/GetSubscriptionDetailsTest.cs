using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Web.AcceptanceTests.Utils;
using VibraHeka.Web.Subscriptions;

namespace VibraHeka.Web.AcceptanceTests.Subscription;

[TestFixture]
public class GetSubscriptionDetailsTest : GenericSubscriptionAcceptanceTest
{
    [Test]
    public async Task ShouldReturnUnauthorizedWhenNotAuthenticated()
    {
        // Given: no authenticated user.
        Client.DefaultRequestHeaders.Remove("Authorization");

        // When: requesting subscription details.
        HttpResponseMessage response = await Client.GetAsync("/api/v1/subscriptions");

        // Then: endpoint returns unauthorized.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenUserHasNoSubscription()
    {
        // Given: an authenticated user without subscription rows.
        await AuthenticateAsConfirmedUser();

        // When: requesting current subscription details.
        HttpResponseMessage response = await Client.GetAsync("/api/v1/subscriptions");

        // Then: error mapping should be NoSubscriptionFound.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        BadRequestResponse? entity = await response.Content.ReadFromJsonAsync<BadRequestResponse>();
        Assert.That(entity!.ErrorCode, Is.EqualTo(SubscriptionErrors.NoSubscriptionFound));
    }

    [Test]
    public async Task ShouldReturnSubscriptionDetailsWhenSubscriptionExists()
    {
        // Given: an authenticated user that already started the subscription flow.
        await AuthenticateAsConfirmedUser();
        HttpResponseMessage subscribeResponse = await Client.PutAsync("/api/v1/subscriptions", null);
        Assert.That(subscribeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // When: requesting subscription details.
        HttpResponseMessage response = await Client.GetAsync("/api/v1/subscriptions");

        // Then: the details DTO should be returned successfully.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        SubscriptionDetailsResponse entity = await response.ParseContentAsync<SubscriptionDetailsResponse>();
        Assert.That(entity.CheckoutSessionUrl, Is.Not.Null.And.Not.Empty);
        Assert.That(entity.CheckoutSessionExpiresAt, Is.GreaterThan(DateTimeOffset.UtcNow.AddHours(-1)));
    }
}
