using System.ComponentModel;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NUnit.Framework;
using VibraHeka.Application.Catalog.Models;
using VibraHeka.Domain.Catalog.Errors;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Web.Authentication;

namespace VibraHeka.Web.AcceptanceTests.Catalog;

/// <summary>
/// Acceptance tests for DELETE /api/v1/recordings/{recordingId}.
/// Tests that require authentication use Cognito and therefore require AWS to be available.
/// </summary>
[TestFixture]
public sealed class DeleteRecordingTest : GenericRecordingsTest
{
    private const string RecordingsBaseEndpoint = "/api/v1/catalog/recordings";
    private const string UploadEndpoint = RecordingsBaseEndpoint;

    private static string BuildDeleteEndpoint(string recordingId) =>
        $"{RecordingsBaseEndpoint}/{recordingId}";

    #region Authentication Tests

    [Test]
    [DisplayName("Should return 401 when no authentication token is provided")]
    public async Task ShouldReturn401WhenNoAuthenticationToken()
    {
        // Given: no authentication token is set on the client
        Client.DefaultRequestHeaders.Remove("Authorization");
        string fakeRecordingId = Guid.NewGuid().ToString();

        // When: calling the delete endpoint without a bearer token
        HttpResponseMessage response = await Client.DeleteAsync(BuildDeleteEndpoint(fakeRecordingId));

        // Then: the response should be 401 Unauthorized
        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.Unauthorized),
            $"Expected 401 Unauthorized when no token is provided, but got {(int)response.StatusCode} {response.StatusCode}");
    }

    #endregion

    #region Validation Tests (CA1 — requires AWS/Cognito for auth)

    [Test]
    [DisplayName("Should return 400 with REC-002 when recordingId is not a valid GUID")]
    public async Task ShouldReturn400WithInvalidRecordingIdErrorWhenRecordingIdIsNotAValidGuid()
    {
        // Given: an authenticated admin and a recording ID with an invalid GUID format
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Person.FullName, email, ThePassword);
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        string invalidId = "not-a-valid-guid";

        // When: calling the delete endpoint with an invalid recording ID
        HttpResponseMessage response = await Client.DeleteAsync(BuildDeleteEndpoint(invalidId));

        // Then: the response should be 400 Bad Request with InvalidRecordingId error
        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.BadRequest),
            $"Expected 400 Bad Request for invalid recording ID but got {(int)response.StatusCode} {response.StatusCode}");

        ResponseEntity entity = await response.GetAsResponseEntity();

        Assert.That(
            entity.Success,
            Is.False,
            "Expected ResponseEntity.Success=false for invalid recording ID but got true");

        Assert.That(
            entity.ErrorCode,
            Does.Contain(RecordingErrors.InvalidRecordingId),
            $"Expected error code to contain '{RecordingErrors.InvalidRecordingId}' but got: '{entity.ErrorCode}'");
    }

    #endregion

    #region Not Found Tests (CA2 — requires AWS)

    [Test]
    [DisplayName("Should return 404 with REC-001 when recording does not exist in DynamoDB")]
    public async Task ShouldReturn404WithNotFoundErrorWhenRecordingDoesNotExist()
    {
        // Given: an authenticated admin and a valid GUID that does not correspond to any recording
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Person.FullName, email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        string nonExistentId = Guid.NewGuid().ToString();

        // When: calling the delete endpoint with a non-existent recording ID
        HttpResponseMessage response = await Client.DeleteAsync(BuildDeleteEndpoint(nonExistentId));

        // Then: the response should be 404 Not Found with NotFound error
        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.NotFound),
            $"Expected 404 Not Found for a non-existent recording but got {(int)response.StatusCode} {response.StatusCode}");

        ResponseEntity entity = await response.GetAsResponseEntity();

        Assert.That(
            entity.Success,
            Is.False,
            "Expected ResponseEntity.Success=false for non-existent recording but got true");

        Assert.That(
            entity.ErrorCode,
            Does.Contain(RecordingErrors.NotFound),
            $"Expected error code to contain '{RecordingErrors.NotFound}' but got: '{entity.ErrorCode}'");
    }

    #endregion

    #region Happy Path Tests (CA3 — requires AWS)

    [Test]
    [DisplayName("Should return 204 NoContent when an existing recording is successfully deleted")]
    public async Task ShouldReturn204NoContentWhenRecordingIsSuccessfullyDeleted()
    {
        // Given: an admin uploads a recording
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Person.FullName, email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        HttpResponseMessage uploadResponse = await Client.PostAsJsonAsync(UploadEndpoint, BuildValidBody());
        uploadResponse.EnsureSuccessStatusCode();

        ResponseEntity uploadEntity = await uploadResponse.GetAsResponseEntityAndContentAs<AddRecordingResult>();
        AddRecordingResult? result = uploadEntity.GetContentAs<AddRecordingResult>();

        Assert.That(
            result,
            Is.Not.Null,
            $"Expected a non-empty recording result after upload but got: '{result}'");

        // When: deleting the uploaded recording
        HttpResponseMessage response = await Client.DeleteAsync(BuildDeleteEndpoint(result.RecordingId));

        // Then: the response should be 204 NoContent with no body
        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.NoContent),
            $"Expected 204 NoContent when deleting an existing recording but got {(int)response.StatusCode} {response.StatusCode}");

        string responseBody = await response.Content.ReadAsStringAsync();

        Assert.That(
            responseBody,
            Is.Empty,
            $"Expected empty body for 204 NoContent but got: '{responseBody}'");
    }

    #endregion
}


