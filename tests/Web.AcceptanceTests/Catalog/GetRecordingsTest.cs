using System.Net;
using System.Net.Http.Headers;
using NUnit.Framework;
using VibraHeka.Application.Catalog.Models;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results;

namespace VibraHeka.Web.AcceptanceTests.Catalog;

[TestFixture]
public sealed class GetRecordingsTest : GenericRecordingsTest
{
    private const string GetRecordingsEndpoint = "/api/v1/catalog/recordings";

    [Test]
    public async Task ShouldReturn401WhenNoAuthenticationToken()
    {
        // Given: no authentication token is set on the client
        Client.DefaultRequestHeaders.Remove("Authorization");

        // When: calling the GET recordings endpoint without a bearer token
        HttpResponseMessage response = await Client.GetAsync(GetRecordingsEndpoint);

        // Then: the response should be 401 Unauthorized
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized),
            $"Expected 401 Unauthorized when no token is provided, but got {(int)response.StatusCode} {response.StatusCode}");
    }

    [Test]
    public async Task ShouldReturn200WhenAuthenticatedAsNonAdminUser()
    {
        // Given: a regular (non-admin) user registered and authenticated
        // Note: GET /api/v1/recordings uses [Authorize] only (no role restriction),
        // so any authenticated user — admin or not — is allowed to call this endpoint.
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(TheFaker.Person.FullName, email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);

        // And: the client uses the non-admin bearer token
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: calling the GET recordings endpoint
        HttpResponseMessage response = await Client.GetAsync(GetRecordingsEndpoint);

        // Then: the response should be 200 OK because any authenticated user has read access
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            $"Expected 200 OK for a non-admin authenticated user on GET recordings endpoint, but got {(int)response.StatusCode} {response.StatusCode}");
    }

    [Test]
    public async Task ShouldReturn200WhenAuthenticatedAsAdmin()
    {
        // Given: an admin user registered and authenticated
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Person.FullName, email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);

        // And: the client uses the admin bearer token
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: calling the GET recordings endpoint
        HttpResponseMessage response = await Client.GetAsync(GetRecordingsEndpoint);

        // Then: the response should be 200 OK
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            $"Expected 200 OK for an authenticated admin but got {(int)response.StatusCode} {response.StatusCode}");
    }

    [Test]
    public async Task ShouldReturnSuccessTrueWithValidCollectionWhenAdminAuthenticated()
    {
        // Given: an admin user registered and authenticated
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Person.FullName, email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: calling the GET recordings endpoint
        HttpResponseMessage response = await Client.GetAsync(GetRecordingsEndpoint);

        // Then: the response should be 200 OK
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            $"Expected 200 OK but got {(int)response.StatusCode} {response.StatusCode}");

        // And: the ResponseEntity should have Success=true
        ResponseEntity entity = await response.GetAsResponseEntityAndContentAs<List<RecordingDto>>();

        Assert.That(entity.Success, Is.True,
            $"Expected ResponseEntity.Success=true but got false. ErrorCode: '{entity.ErrorCode}'");

        // And: the content should be a non-null recordings collection
        List<RecordingDto>? recordings = entity.GetContentAs<List<RecordingDto>>();

        Assert.That(recordings, Is.Not.Null,
            "Expected a non-null recordings collection in the response content but got null");
    }
}
