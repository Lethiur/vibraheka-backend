using System.ComponentModel;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using NUnit.Framework;
using VibraHeka.Application.Recordings.Entities;
using VibraHeka.Application.Recordings.Queries.GetRecordingDownloadUrl;
using VibraHeka.Domain.Commerce.Enums;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Domain.Recordings.Errors;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.Recordings;

[TestFixture]
public sealed class GetDownloadUrlTest : GenericRecordingsTest
{
    private const string RecordingsBaseEndpoint = "/api/v1/catalog/recordings";
    private static readonly HttpClient S3Client = new();
    private const string UploadEndpoint = RecordingsBaseEndpoint;

    private static string BuildDownloadUrlEndpoint(string recordingId) =>
        $"{RecordingsBaseEndpoint}/{recordingId}/download-url";

    [Test]
    [DisplayName("Should return 401 when no authentication token is provided")]
    public async Task ShouldReturn401WhenNoAuthenticationToken()
    {
        // Given: no authentication token is set on the client
        Client.DefaultRequestHeaders.Remove("Authorization");
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
            await Client.PostAsJsonAsync(UploadEndpoint, BuildValidBody());
        uploadResponse.EnsureSuccessStatusCode();
        ResponseEntity uploadEntity = await uploadResponse.GetAsResponseEntityAndContentAs<AddRecordingResult>();
        AddRecordingResult? recordingId = uploadEntity.GetContentAs<AddRecordingResult>();
        Assert.That(recordingId, Is.Not.Null,
            $"Expected a non-null recording result after upload but got: '{recordingId}'");

        // When: requesting the download URL for the uploaded recording
        HttpResponseMessage response =
            await Client.GetAsync(BuildDownloadUrlEndpoint(recordingId!.RecordingId));

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
    [DisplayName("Should return 404 when recording does not exist")]
    public async Task ShouldReturn404WhenRecordingDoesNotExist()
    {
        // Given: an authenticated user and a recording ID that does not exist in the system
        string email = TheFaker.Internet.Email();
        AuthenticationResult auth = await RegisterConfirmAndLogin(email, email, ThePassword);
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
        AuthenticationResult auth = await RegisterConfirmAndLogin(email, email, ThePassword);
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

    [Test]
    [DisplayName("Should return 400 with subscription error when premium recording and user has no subscription")]
    public async Task ShouldReturn400WhenPremiumRecordingAndUserHasNoSubscription()
    {
        // Given: an admin uploads a premium recording
        string adminEmail = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Person.FullName, adminEmail, ThePassword);
        AuthenticationResult adminAuth = await AuthenticateUser(adminEmail, ThePassword);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", adminAuth.AccessToken);

        HttpResponseMessage uploadResponse =
            await Client.PostAsJsonAsync(UploadEndpoint, BuildPremiumBody());
        uploadResponse.EnsureSuccessStatusCode();
        ResponseEntity uploadEntity = await uploadResponse.GetAsResponseEntityAndContentAs<AddRecordingResult>();
        AddRecordingResult? recordingResult = uploadEntity.GetContentAs<AddRecordingResult>();
        Assert.That(recordingResult, Is.Not.Null,
            "Expected a non-null recording result after premium upload but got null");

        // Given: a regular user with no subscription authenticates
        string userEmail = TheFaker.Internet.Email();
        AuthenticationResult userAuth = await RegisterConfirmAndLogin(TheFaker.Person.FullName, userEmail, ThePassword);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", userAuth.AccessToken);

        // When: the user without subscription requests the download URL for the premium recording
        HttpResponseMessage response =
            await Client.GetAsync(BuildDownloadUrlEndpoint(recordingResult!.RecordingId));

        // Then: the response should be 400 BadRequest, no subscription found error propagated
        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.BadRequest),
            $"Expected 400 BadRequest for premium recording without subscription but got {(int)response.StatusCode} {response.StatusCode}");

        ResponseEntity entity = await response.GetAsResponseEntity();

        Assert.That(
            entity.Success,
            Is.False,
            $"Expected ResponseEntity.Success=false when user has no subscription but got true. ErrorCode: '{entity.ErrorCode}'");

        Assert.That(
            entity.ErrorCode,
            Is.EqualTo(SubscriptionErrors.NoSubscriptionFound),
            $"Expected error code '{SubscriptionErrors.NoSubscriptionFound}' (no subscription) but got: '{entity.ErrorCode}'");

        Assert.That(
            entity.Content,
            Is.Null,
            "Expected no DownloadUrl content when access is denied due to missing subscription");
    }

    [Test]
    [DisplayName("Should return 400 with REC-003 when premium recording and user subscription is not active")]
    public async Task ShouldReturn400WhenPremiumRecordingAndSubscriptionIsNotActive()
    {
        // Given: an admin uploads a premium recording
        string adminEmail = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Person.FullName, adminEmail, ThePassword);
        AuthenticationResult adminAuth = await AuthenticateUser(adminEmail, ThePassword);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", adminAuth.AccessToken);

        HttpResponseMessage uploadResponse =
            await Client.PostAsJsonAsync(UploadEndpoint, BuildPremiumBody());
        uploadResponse.EnsureSuccessStatusCode();
        ResponseEntity uploadEntity = await uploadResponse.GetAsResponseEntityAndContentAs<AddRecordingResult>();
        AddRecordingResult? recordingResult = uploadEntity.GetContentAs<AddRecordingResult>();
        Assert.That(recordingResult, Is.Not.Null,
            "Expected a non-null recording result after premium upload but got null");

        // Given: a regular user registers, logs in and has a cancelled (inactive) subscription
        string userEmail = TheFaker.Internet.Email();
        AuthenticationResult userAuth = await RegisterConfirmAndLogin(TheFaker.Person.FullName, userEmail, ThePassword);

        Result<SubscriptionEntity> seedResult =
            await SeedSubscriptionForRecordingTest(userAuth.UserID, SubscriptionStatus.Cancelled, OrderStatus.Cancelled);
        Assert.That(seedResult.IsSuccess, Is.True,
            $"Subscription seeding should succeed but got error: '{(seedResult.IsFailure ? seedResult.Error : "N/A")}'");

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", userAuth.AccessToken);

        // When: the user with an inactive subscription requests the download URL for the premium recording
        HttpResponseMessage response =
            await Client.GetAsync(BuildDownloadUrlEndpoint(recordingResult!.RecordingId));

        // Then: the response should be 400 BadRequest with OnlyForSubscribers error
        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.BadRequest),
            $"Expected 400 BadRequest for premium recording with inactive subscription but got {(int)response.StatusCode} {response.StatusCode}");

        ResponseEntity entity = await response.GetAsResponseEntity();

        Assert.That(
            entity.Success,
            Is.False,
            $"Expected ResponseEntity.Success=false when subscription is not active but got true. ErrorCode: '{entity.ErrorCode}'");

        Assert.That(
            entity.ErrorCode,
            Is.EqualTo(RecordingErrors.OnlyForSubscribers),
            $"Expected error code '{RecordingErrors.OnlyForSubscribers}' (only for subscribers) but got: '{entity.ErrorCode}'");

        Assert.That(
            entity.Content,
            Is.Null,
            "Expected no DownloadUrl content when access is denied due to inactive subscription");
    }

    [Test]
    [DisplayName("Should return 200 with non-empty URL when premium recording and user has active subscription")]
    public async Task ShouldReturn200WhenPremiumRecordingAndSubscriptionIsActive()
    {
        // Given: an admin uploads a premium recording
        string adminEmail = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Person.FullName, adminEmail, ThePassword);
        AuthenticationResult adminAuth = await AuthenticateUser(adminEmail, ThePassword);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", adminAuth.AccessToken);

        HttpResponseMessage uploadResponse =
            await Client.PostAsJsonAsync(UploadEndpoint, BuildPremiumBody());
        uploadResponse.EnsureSuccessStatusCode();
        ResponseEntity uploadEntity = await uploadResponse.GetAsResponseEntityAndContentAs<AddRecordingResult>();
        AddRecordingResult? recordingResult = uploadEntity.GetContentAs<AddRecordingResult>();
        Assert.That(recordingResult, Is.Not.Null,
            "Expected a non-null recording result after premium upload but got null");

        // Given: a regular user registers, logs in and has an active subscription
        string userEmail = TheFaker.Internet.Email();
        AuthenticationResult userAuth = await RegisterConfirmAndLogin(TheFaker.Person.FullName, userEmail, ThePassword);

        Result<SubscriptionEntity> seedResult =
            await SeedSubscriptionForRecordingTest(userAuth.UserID, SubscriptionStatus.Active, OrderStatus.Paid);
        Assert.That(seedResult.IsSuccess, Is.True,
            $"Subscription seeding should succeed but got error: '{(seedResult.IsFailure ? seedResult.Error : "N/A")}'");

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", userAuth.AccessToken);

        // When: the user with an active subscription requests the download URL for the premium recording
        HttpResponseMessage response =
            await Client.GetAsync(BuildDownloadUrlEndpoint(recordingResult!.RecordingId));

        // Then: the response should be 200 OK with a non-empty download URL
        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.OK),
            $"Expected 200 OK for premium recording with active subscription but got {(int)response.StatusCode} {response.StatusCode}");

        ResponseEntity entity = await response.GetAsResponseEntityAndContentAs<RecordingDownloadUrlDto>();

        Assert.That(
            entity.Success,
            Is.True,
            $"Expected ResponseEntity.Success=true for active subscriber but got false. ErrorCode: '{entity.ErrorCode}'");

        RecordingDownloadUrlDto? dto = entity.GetContentAs<RecordingDownloadUrlDto>();

        Assert.That(
            dto,
            Is.Not.Null,
            "Expected a non-null RecordingDownloadUrlDto in the response content but got null");

        Assert.That(
            dto!.DownloadUrl,
            Is.Not.Null.And.Not.Empty,
            $"Expected a non-empty DownloadUrl for active subscriber but got: '{dto.DownloadUrl}'");
    }
}
