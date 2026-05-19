using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NUnit.Framework;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.Catalog;

[TestFixture]
public sealed class CreateProductTest : GenericCatalogAcceptanceTest
{
    [Test]
    public async Task ShouldReturn403WhenUserIsNotAdmin()
    {
        // When: calling the create product with non admin user
        HttpResponseMessage response = await Client.PostAsJsonAsync(CatalogEndpoint, BuildValidRequest());

        // Then: the response should be 401 Unauthorized
        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.Unauthorized),
            $"Expected 401 Unauthorized when no token is provided, but got {(int)response.StatusCode} {response.StatusCode}");
    }

    [Test]
    public async Task ShouldReturn200WithProductIdWhenAdminSubmitsValidPayload()
    {
        // Given: an admin user registered and authenticated
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Person.FullName, email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: calling the create product endpoint with a valid JSON payload
        HttpResponseMessage response = await Client.PostAsJsonAsync(CatalogEndpoint, BuildValidRequest());

        // Then: the response should be 200 OK
        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.OK),
            $"Expected 200 OK for a valid admin create product request but got {(int)response.StatusCode} {response.StatusCode}");

        // And: the response body should contain Success=true with a non-empty Guid product ID
        ResponseEntity entity = await response.GetAsResponseEntityAndContentAs<string>();

        Assert.That(
            entity.Success,
            Is.True,
            $"Expected ResponseEntity.Success=true but got false. ErrorCode: '{entity.ErrorCode}'");

        string? productId = entity.GetContentAs<string>();

        Assert.That(
            productId,
            Is.Not.Null.And.Not.Empty,
            $"Expected a non-empty product ID in the response content but got: '{productId}'");

        Assert.That(
            Guid.TryParse(productId, out _),
            Is.True,
            $"Expected product ID to be a valid Guid but got: '{productId}'");
    }
}



