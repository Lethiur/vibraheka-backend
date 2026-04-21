using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NUnit.Framework;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Domain.Recordings.Errors;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.Recordings;

[TestFixture]
public class UploadRecordingTest : GenericAcceptanceTest<VibraHekaProgram>
{
    private const string UploadEndpoint = "/api/v1/recordings";

    [Test]
    public async Task ShouldReturn401WhenNoAuthenticationToken()
    {
        // Given: no authentication token is set on the client

        // When: calling the upload endpoint without a bearer token
        HttpResponseMessage response = await Client.PostAsync(UploadEndpoint, RecordingAcceptanceHelpers.BuildValidBody());

        // Then: the response should be 401 Unauthorized
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized),
            $"Expected 401 Unauthorized when no token is provided, but got {(int)response.StatusCode} {response.StatusCode}");
    }

    [Test]
    public async Task ShouldReturn401WhenAuthenticatedAsNonAdminUser()
    {
        // Given: a regular (non-admin) user registered and authenticated
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(TheFaker.Person.FullName, email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);

        // And: the client uses the non-admin bearer token
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: calling the upload endpoint
        HttpResponseMessage response = await Client.PostAsync(UploadEndpoint, RecordingAcceptanceHelpers.BuildValidBody());

        // Then: the response should be 401 or 403 because the user is not an admin
        bool isAccessDenied =
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.Forbidden;

        Assert.That(isAccessDenied, Is.True,
            $"Expected 401 or 403 when non-admin user calls upload endpoint, but got {(int)response.StatusCode} {response.StatusCode}");
    }

    [Test]
    public async Task ShouldReturn200WithRecordingIdWhenAdminUploadsValidFile()
    {
        // Given: an admin user registered and authenticated
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Person.FullName, email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: calling the upload endpoint with a valid JSON body
        HttpResponseMessage response = await Client.PostAsync(UploadEndpoint, RecordingAcceptanceHelpers.BuildValidBody());

        // Then: the response should be 200 OK
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            $"Expected 200 OK for a valid admin upload but got {(int)response.StatusCode} {response.StatusCode}");

        // And: the response body should contain a non-empty recording ID
        ResponseEntity entity = await response.GetAsResponseEntityAndContentAs<string>();

        Assert.That(entity.Success, Is.True,
            $"Expected ResponseEntity.Success=true but got false. ErrorCode: '{entity.ErrorCode}'");

        string? recordingId = entity.GetContentAs<string>();
        Assert.That(recordingId, Is.Not.Null.And.Not.Empty,
            $"Expected a non-empty recording ID in the response content but got: '{recordingId}'");
    }

    [Test]
    public async Task ShouldReturn400WhenFileBase64IsEmpty()
    {
        // Given: an authenticated admin
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Person.FullName, email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: calling the upload endpoint with an empty file
        HttpResponseMessage response = await Client.PostAsync(
            UploadEndpoint,
            RecordingAcceptanceHelpers.BuildBodyWithFile(Array.Empty<byte>(), "empty.mp4"));

        // Then: the response should be 400 Bad Request
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            $"Expected 400 Bad Request for empty file but got {(int)response.StatusCode} {response.StatusCode}");

        ResponseEntity entity = await response.GetAsResponseEntity();

        Assert.That(entity.Success, Is.False,
            "Expected ResponseEntity.Success=false for empty file but got true");
        Assert.That(entity.ErrorCode, Does.Contain(RecordingErrors.InvalidFile),
            $"Expected error code to contain '{RecordingErrors.InvalidFile}' but got: '{entity.ErrorCode}'");
    }

    [Test]
    public async Task ShouldReturn400WithInvalidNameErrorWhenNameIsEmpty()
    {
        // Given: an authenticated admin
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Person.FullName, email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: calling the upload endpoint with an empty name
        HttpResponseMessage response = await Client.PostAsync(
            UploadEndpoint,
            RecordingAcceptanceHelpers.BuildBody(name: ""));

        // Then: the response should be 400 Bad Request with InvalidName error
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            $"Expected 400 Bad Request for empty name but got {(int)response.StatusCode} {response.StatusCode}");

        ResponseEntity entity = await response.GetAsResponseEntity();

        Assert.That(entity.Success, Is.False,
            "Expected ResponseEntity.Success=false for empty name but got true");
        Assert.That(entity.ErrorCode, Does.Contain(RecordingErrors.InvalidName),
            $"Expected error code to contain '{RecordingErrors.InvalidName}' but got: '{entity.ErrorCode}'");
    }

    [Test]
    public async Task ShouldReturn400WithInvalidDescriptionErrorWhenDescriptionIsEmpty()
    {
        // Given: an authenticated admin
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Person.FullName, email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: calling the upload endpoint with an empty description
        HttpResponseMessage response = await Client.PostAsync(
            UploadEndpoint,
            RecordingAcceptanceHelpers.BuildBody(description: ""));

        // Then: the response should be 400 Bad Request with InvalidDescription error
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            $"Expected 400 Bad Request for empty description but got {(int)response.StatusCode} {response.StatusCode}");

        ResponseEntity entity = await response.GetAsResponseEntity();

        Assert.That(entity.Success, Is.False,
            "Expected ResponseEntity.Success=false for empty description but got true");
        Assert.That(entity.ErrorCode, Does.Contain(RecordingErrors.InvalidDescription),
            $"Expected error code to contain '{RecordingErrors.InvalidDescription}' but got: '{entity.ErrorCode}'");
    }
}
