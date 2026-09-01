using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NUnit.Framework;
using VibraHeka.Web.AcceptanceTests.Utils;
using VibraHeka.Web.Authentication;
using VibraHeka.Web.Catalog.Orders.Controllers;

namespace VibraHeka.Web.AcceptanceTests.Commerce;

[TestFixture]
public class CreateOrderTest : GenericOrdersTest
{
    [Test]
    public async Task ShouldReturn401WhenNoAuthenticationToken()
    {
        // Given: no authentication token is set on the client

        // When: calling the orders endpoint without a bearer token
        HttpResponseMessage response = await Client.PostAsJsonAsync(OrdersEndpoint, BuildValidRequest(Guid.NewGuid()));

        // Then: the response should be 401 Unauthorized
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized),
            $"Expected 401 Unauthorized when no token is provided, but got {(int)response.StatusCode} {response.StatusCode}");
    }

    [Test]
    public async Task ShouldReturn400WhenOrderLinesIsEmpty()
    {
        // Given: an authenticated user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(TheFaker.Person.FullName, email, ThePassword);
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: calling the orders endpoint with empty order lines
        HttpResponseMessage response = await Client.PostAsJsonAsync(OrdersEndpoint, BuildRequestWithNoLines(Guid.NewGuid()));

        // Then: the response should be 400 Bad Request
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            $"Expected 400 BadRequest for empty order lines but got {(int)response.StatusCode} {response.StatusCode}");
    }

    [Test]
    public async Task ShouldReturn400WhenIdempotencyKeyIsEmpty()
    {
        // Given: an authenticated user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(TheFaker.Person.FullName, email, ThePassword);
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: calling the orders endpoint with an empty IdempotencyKey
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            OrdersEndpoint,
            BuildRequestWithEmptyIdempotencyKey());

        // Then: the response should be 400 Bad Request (validator rejects)
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            $"Expected 400 BadRequest for empty IdempotencyKey but got {(int)response.StatusCode} {response.StatusCode}");
    }

    [Test]
    public async Task ShouldReturn400WhenSellableItemIdIsEmptyInALine()
    {
        // Given: an authenticated user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(TheFaker.Person.FullName, email, ThePassword);
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: calling the orders endpoint with an empty SellableItemID in a line
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            OrdersEndpoint,
            BuildRequestWithEmptySellableItemId());

        // Then: the response should be 400 Bad Request (validator rejects)
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            $"Expected 400 BadRequest when SellableItemID is empty but got {(int)response.StatusCode} {response.StatusCode}");
    }

    [Test]
    public async Task ShouldReturn400WhenSellableItemPriceIdIsEmptyInALine()
    {
        // Given: an authenticated user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(TheFaker.Person.FullName, email, ThePassword);
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: calling the orders endpoint with an empty SellableItemPriceID in a line
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            OrdersEndpoint,
            BuildRequestWithEmptySellableItemPriceId());

        // Then: the response should be 400 Bad Request (validator rejects)
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            $"Expected 400 BadRequest when SellableItemPriceID is empty but got {(int)response.StatusCode} {response.StatusCode}");
    }

    [Test]
    public async Task ShouldReturn400WhenQuantityIsZeroInALine()
    {
        // Given: an authenticated user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(TheFaker.Person.FullName, email, ThePassword);
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: calling the orders endpoint with Quantity=0 in a line
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            OrdersEndpoint,
            BuildRequestWithZeroQuantity());

        // Then: the response should be 400 Bad Request (validator rejects)
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            $"Expected 400 BadRequest when Quantity=0 but got {(int)response.StatusCode} {response.StatusCode}");
    }

    [Test]
    public async Task ShouldReturn201WithCreateOrderResponseWhenRequestIsValid()
    {
        // Given: a catalog product is seeded via the catalog endpoint before placing the order
        (Guid sellableItemId, Guid sellableItemPriceId) = await SeedCatalogProductAsync();

        // And: an authenticated user with JWT
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(TheFaker.Person.FullName, email, ThePassword);
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: calling the orders endpoint with a valid request using the seeded IDs
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            OrdersEndpoint,
            BuildValidRequest(sellableItemId, sellableItemPriceId, Guid.NewGuid()));

        // Then: the response should be 201 Created with CreateOrderResponse content
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created),
            $"Expected 201 Created for valid order request but got {(int)response.StatusCode} {response.StatusCode}");

        CreateOrderResponse entity = await response.ParseContentAsync<CreateOrderResponse>();
       
        Assert.That(entity, Is.Not.Null,
            "Expected CreateOrderResponse in content but got null");
        Assert.That(entity.CheckoutUrl, Is.Not.Null.And.Not.Empty,
            $"Expected a non-empty CheckoutURL but got: '{entity.CheckoutUrl}'");
    }
}

