using System.ComponentModel;
using System.Net;
using System.Net.Http.Headers;
using NUnit.Framework;
using VibraHeka.Application.Recordings.Queries.GetRecordingDownloadUrl;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Domain.Recordings.Errors;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.Recordings;

[TestFixture]
public sealed class GetDownloadUrlTest : GenericRecordingsTest
{
    private const string RecordingsBaseEndpoint = "/api/v1/recordings";
    private static readonly HttpClient S3Client = new();
    private const string UploadEndpoint = RecordingsBaseEndpoint;

    private static string BuildDownloadUrlEndpoint(string recordingId) =>
        $"{RecordingsBaseEndpoint}/{recordingId}/download-url";

    [Test]
    [DisplayName("Should return 401 when no authentication token is provided")]
    public async Task ShouldReturn401WhenNoAuthenticationToken()
    {
        // Given: no authentication token is set on the client
        string fakeRecordingId = Guid.NewGuid().ToString();

        // When: calling the download-url endpoint without a bearer token
        HttpResponseMessage response = await Client.GetAsync(BuildDownloadUrlEndpoint(fakeRecordingId));

        // Then: the response should be 401 Unauthorized
        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.Unauthorized),
            $"Expected 401 Unauthorized when no token is provided, but got {(int)response.StatusCode} {response.StatusCode}");
    }

    [Test]
    [DisplayName("Should return 200 with non-empty URL when recording exists")]
    public async Task ShouldReturn200WithNonEmptyUrlWhenRecordingExists()
    {
        // Given: an admin user uploads a recording
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Person.FullName, email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        HttpResponseMessage uploadResponse =
            await Client.PostAsync(UploadEndpoint, BuildValidBody());
        uploadResponse.EnsureSuccessStatusCode();
        ResponseEntity uploadEntity =
            await uploadResponse.GetAsResponseEntityAndContentAs<string>();
        string? recordingId = uploadEntity.GetContentAs<string>();
        Assert.That(
            recordingId,
            Is.Not.Null.And.Not.Empty,
            $"Expected a non-empty recording ID after upload but got: '{recordingId}'");

        // When: requesting the download URL for the uploaded recording
        HttpResponseMessage response =
            await Client.GetAsync(BuildDownloadUrlEndpoint(recordingId!));

        // Then: the response should be 200 OK
        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.OK),
            $"Expected 200 OK for an existing recording but got {(int)response.StatusCode} {response.StatusCode}");

        ResponseEntity entity =
            await response.GetAsResponseEntityAndContentAs<RecordingDownloadUrlDto>();

        Assert.That(
            entity.Success,
            Is.True,
            $"Expected ResponseEntity.Success=true but got false. ErrorCode: '{entity.ErrorCode}'");

        RecordingDownloadUrlDto? dto = entity.GetContentAs<RecordingDownloadUrlDto>();

        Assert.That(
            dto,
            Is.Not.Null,
            "Expected a non-null RecordingDownloadUrlDto in the response content but got null");

        Assert.That(
            dto!.DownloadUrl,
            Is.Not.Null.And.Not.Empty,
            $"Expected a non-empty DownloadUrl but got: '{dto.DownloadUrl}'");
    }

    [Test]
    [DisplayName("Should return exact bytes when downloading content from the signed URL")]
    public async Task ShouldReturnExactBytesWhenDownloadingContentFromSignedUrl()
    {
        // Given: an admin uploads a recording with a known binary payload
        byte[] knownBytes = [0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08];
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Person.FullName, email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        HttpResponseMessage uploadResponse =
            await Client.PostAsync(UploadEndpoint, BuildBodyWithFile(knownBytes, "payload-test.mp4"));
        uploadResponse.EnsureSuccessStatusCode();
        ResponseEntity uploadEntity =
            await uploadResponse.GetAsResponseEntityAndContentAs<string>();
        string? recordingId = uploadEntity.GetContentAs<string>();
        Assert.That(
            recordingId,
            Is.Not.Null.And.Not.Empty,
            $"Expected a non-empty recording ID after upload but got: '{recordingId}'");

        // When: requesting the download URL and downloading the content
        HttpResponseMessage downloadUrlResponse =
            await Client.GetAsync(BuildDownloadUrlEndpoint(recordingId!));
        downloadUrlResponse.EnsureSuccessStatusCode();
        ResponseEntity downloadUrlEntity =
            await downloadUrlResponse.GetAsResponseEntityAndContentAs<RecordingDownloadUrlDto>();
        RecordingDownloadUrlDto? dto = downloadUrlEntity.GetContentAs<RecordingDownloadUrlDto>();
        Assert.That(
            dto,
            Is.Not.Null,
            "Expected a non-null RecordingDownloadUrlDto in the response content but got null");
        Assert.That(
            dto!.DownloadUrl,
            Is.Not.Null.And.Not.Empty,
            $"Expected a non-empty DownloadUrl but got: '{dto.DownloadUrl}'");

        HttpResponseMessage s3Response = await S3Client.GetAsync(dto.DownloadUrl);
        // Then: the downloaded bytes must match exactly the bytes that were uploaded
        Assert.That(
            s3Response.IsSuccessStatusCode,
            Is.True,
            $"Expected a successful HTTP response downloading from the signed URL but got {(int)s3Response.StatusCode} {s3Response.StatusCode}");

        byte[] downloadedBytes = await s3Response.Content.ReadAsByteArrayAsync();

        Assert.That(
            downloadedBytes,
            Is.EqualTo(knownBytes),
            $"Expected downloaded content to match uploaded bytes exactly. "
            + $"Uploaded {knownBytes.Length} bytes, downloaded {downloadedBytes.Length} bytes");
    }

    [Test]
    [DisplayName("Should return 404 when recording does not exist")]
    public async Task ShouldReturn404WhenRecordingDoesNotExist()
    {
        // Given: an authenticated user and a recording ID that does not exist in the system
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(TheFaker.Person.FullName, email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        string nonExistentId = Guid.NewGuid().ToString();

        // When: requesting the download URL for the non-existent recording
        HttpResponseMessage response =
            await Client.GetAsync(BuildDownloadUrlEndpoint(nonExistentId));

        // Then: the response should be 404 Not Found
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

    [Test]
    [DisplayName("Should return 400 with invalid recording ID error when recording ID is not a valid GUID")]
    public async Task ShouldReturn400WhenRecordingIdIsNotAValidGuid()
    {
        // Given: an authenticated user and a recording ID that is not a valid GUID
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(TheFaker.Person.FullName, email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        string invalidId = "not-a-valid-guid";

        // When: requesting the download URL with an invalid recording ID format
        HttpResponseMessage response =
            await Client.GetAsync(BuildDownloadUrlEndpoint(invalidId));

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
}





