using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NUnit.Framework;
using VibraHeka.Domain.Catalog.Errors;
using VibraHeka.Web.AcceptanceTests.Utils;
using VibraHeka.Web.Authentication;
using VibraHeka.Web.Catalog.Recordings.Controllers;
using BadRequestResponse = VibraHeka.Web.Catalog.Recordings.Controllers.BadRequestResponse;

namespace VibraHeka.Web.AcceptanceTests.Catalog;

[TestFixture]
public class UploadRecordingTest : GenericRecordingsTest
{
    private const string UploadEndpoint = "/api/v1/catalog/recordings/admin";

    [Test]
    public async Task ShouldReturn401WhenNoAuthenticationToken()
    {
        // Given: no authentication token is set on the client

        // When: calling the upload endpoint without a bearer token
        HttpResponseMessage response = await Client.PostAsJsonAsync(UploadEndpoint, BuildValidBody());

        // Then: the response should be 401 Unauthorized
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized),
            $"Expected 401 Unauthorized when no token is provided, but got {(int)response.StatusCode} {response.StatusCode}");
    }

    [Test]
    public async Task ShouldReturn401WhenAuthenticatedAsNonAdminUser()
    {
        // Given: a regular (non-admin) user registered and authenticated
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(email, ThePassword);
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);

        // And: the client uses the non-admin bearer token
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: calling the upload endpoint
        HttpResponseMessage response = await Client.PutAsJsonAsync(UploadEndpoint, BuildValidBody());

        // Then: the response should be 401 or 403 because the user is not an admin
        bool isAccessDenied =
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.Forbidden;

        Assert.That(isAccessDenied, Is.True,
            $"Expected 401 or 403 when non-admin user calls upload endpoint, but got {(int)response.StatusCode} {response.StatusCode}");
    }

    [Test]
    public async Task ShouldReturn200WithRecordingIdAndUploadUrlWhenAdminSubmitsValidMetadata()
    {
        // Given: an admin user registered and authenticated
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Person.FullName, email, ThePassword);
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: calling the upload endpoint with valid JSON metadata
        HttpResponseMessage response = await Client.PutAsJsonAsync(UploadEndpoint, BuildValidBody());

        // Then: the response should be 200 OK
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            $"Expected 200 OK for a valid admin upload but got {(int)response.StatusCode} {response.StatusCode}");

        // And: the response body should contain a non-empty RecordingId and UploadUrl
        CreateRecordingResponse entity = await response.ParseContentAsync<CreateRecordingResponse>();

        Assert.That(entity.Id, Is.Not.Null.And.Not.Empty,
            $"Expected a non-empty RecordingId but got: '{entity.Id}'");
        Assert.That(entity.UploadUrl, Is.Not.Null.And.Not.Empty,
            $"Expected a non-empty UploadUrl but got: '{entity.UploadUrl}'");
    }

    [Test]
    public async Task ShouldReturn400WithInvalidNameErrorWhenNameIsEmpty()
    {
        // Given: an authenticated admin
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Person.FullName, email, ThePassword);
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: calling the upload endpoint with an empty name
        HttpResponseMessage response = await Client.PostAsJsonAsync(UploadEndpoint, BuildBody(name: ""));

        // Then: the response should be 400 Bad Request with InvalidName error
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            $"Expected 400 Bad Request for empty name but got {(int)response.StatusCode} {response.StatusCode}");

        BadRequestResponse entity = await response.ParseContentAsync<BadRequestResponse>();
        Assert.That(entity.ErrorCode, Does.Contain(RecordingErrors.InvalidName),
            $"Expected error code to contain '{RecordingErrors.InvalidName}' but got: '{entity.ErrorCode}'");
    }

    [Test]
    public async Task ShouldReturn400WithInvalidDescriptionErrorWhenDescriptionIsEmpty()
    {
        // Given: an authenticated admin
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Person.FullName, email, ThePassword);
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: calling the upload endpoint with an empty description
        HttpResponseMessage response = await Client.PostAsJsonAsync(UploadEndpoint, BuildBody(description: ""));

        // Then: the response should be 400 Bad Request with InvalidDescription error
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            $"Expected 400 Bad Request for empty description but got {(int)response.StatusCode} {response.StatusCode}");

        BadRequestResponse entity = await response.ParseContentAsync<BadRequestResponse>();
        Assert.That(entity.ErrorCode, Does.Contain(RecordingErrors.InvalidDescription),
            $"Expected error code to contain '{RecordingErrors.InvalidDescription}' but got: '{entity.ErrorCode}'");
    }

    [Test]
    public async Task ShouldReturn400WithInvalidTypeErrorWhenTypeIsOutOfRange()
    {
        // Given: an authenticated admin
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Person.FullName, email, ThePassword);
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: calling the upload endpoint with an invalid type value
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            UploadEndpoint,
            BuildBody(type: (RecordingType)999));

        // Then: the response should be 400 Bad Request with InvalidType error
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            $"Expected 400 Bad Request for invalid type but got {(int)response.StatusCode} {response.StatusCode}");

        BadRequestResponse entity = await response.ParseContentAsync<BadRequestResponse>();
        
        Assert.That(entity.ErrorCode, Does.Contain(RecordingErrors.InvalidType),
            $"Expected error code to contain '{RecordingErrors.InvalidType}' but got: '{entity.ErrorCode}'");
    }
}
