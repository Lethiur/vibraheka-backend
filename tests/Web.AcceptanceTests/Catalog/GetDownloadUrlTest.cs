using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using NUnit.Framework;
using VibraHeka.Domain.Catalog.Errors;
using VibraHeka.Domain.Commerce.Enums;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Web.AcceptanceTests.Utils;
using VibraHeka.Web.Authentication;
using VibraHeka.Web.Catalog.Recordings.Controllers;
using BadRequestResponse = VibraHeka.Web.Authentication.BadRequestResponse;

namespace VibraHeka.Web.AcceptanceTests.Catalog;

[TestFixture]
public sealed class GetDownloadUrlTest : GenericRecordingsTest
{
    private const string RecordingsBaseEndpoint = "/api/v1/catalog/recordings";
    private const string RecordingAdminBaseEndpoint = RecordingsBaseEndpoint + "/admin";
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
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        HttpResponseMessage uploadResponse =
            await Client.PutAsJsonAsync(RecordingAdminBaseEndpoint, BuildValidBody());
        uploadResponse.EnsureSuccessStatusCode();
        CreateRecordingResponse uploadEntity = await uploadResponse.ParseContentAsync<CreateRecordingResponse>();
        
        Assert.That(uploadEntity.Id, Is.Not.Null,
            $"Expected a non-null recording result after upload but got: '{uploadEntity.Id}'");

        // When: requesting the download URL for the uploaded recording
        HttpResponseMessage response =
            await Client.GetAsync(BuildDownloadUrlEndpoint(uploadEntity.Id.ToString()));

        // Then: the response should be 200 OK
        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.OK),
            $"Expected 200 OK for an existing recording but got {(int)response.StatusCode} {response.StatusCode}");

        GetRecordingDownloadUrlResponse entity =
            await response.ParseContentAsync<GetRecordingDownloadUrlResponse>();

      

        Assert.That(
            entity.DownloadUrl,
            Is.Not.Null.And.Not.Empty,
            $"Expected a non-empty DownloadUrl but got: '{entity.DownloadUrl}'");
    }

    [Test]
    [DisplayName("Should return 404 when recording does not exist")]
    public async Task ShouldReturn404WhenRecordingDoesNotExist()
    {
        // Given: an authenticated user and a recording ID that does not exist in the system
        string email = TheFaker.Internet.Email();
        AuthenticateUserResponse auth = await RegisterConfirmAndLogin(email, email, ThePassword);
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
    }

    [Test]
    [DisplayName("Should return 400 with invalid recording ID error when recording ID is not a valid GUID")]
    public async Task ShouldReturn400WhenRecordingIdIsNotAValidGuid()
    {
        // Given: an authenticated user and a recording ID that is not a valid GUID
        string email = TheFaker.Internet.Email();
        AuthenticateUserResponse auth = await RegisterConfirmAndLogin(email, email, ThePassword);
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

        BadRequestResponse entity = await response.ParseContentAsync<BadRequestResponse>();
        
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
        AuthenticateUserResponse adminAuth = await AuthenticateUser(adminEmail, ThePassword);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", adminAuth.AccessToken);

        HttpResponseMessage uploadResponse = await Client.PutAsJsonAsync(UploadEndpoint, BuildPremiumBody());
        CreateRecordingResponse uploadEntity = await uploadResponse.ParseContentAsync<CreateRecordingResponse>();
        

        // And: a regular user with no subscription authenticates
        string userEmail = TheFaker.Internet.Email();
        AuthenticateUserResponse userAuth = await RegisterConfirmAndLogin(TheFaker.Person.FullName, userEmail, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userAuth.AccessToken);

        // When: the user without subscription requests the download URL for the premium recording
        HttpResponseMessage response = await Client.GetAsync(BuildDownloadUrlEndpoint(uploadEntity.Id.ToString()));

        // Then: the response should be 400 BadRequest, no subscription found error propagated
        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.BadRequest),
            $"Expected 400 BadRequest for premium recording without subscription but got {(int)response.StatusCode} {response.StatusCode}");

        BadRequestResponse entity = await response.ParseContentAsync<BadRequestResponse>();
        
        Assert.That(
            entity.ErrorCode,
            Is.EqualTo(SubscriptionErrors.NoSubscriptionFound),
            $"Expected error code '{SubscriptionErrors.NoSubscriptionFound}' (no subscription) but got: '{entity.ErrorCode}'");

     
    }

    [Test]
    [DisplayName("Should return 400 with REC-003 when premium recording and user subscription is not active")]
    public async Task ShouldReturn400WhenPremiumRecordingAndSubscriptionIsNotActive()
    {
        // Given: an admin uploads a premium recording
        string adminEmail = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Person.FullName, adminEmail, ThePassword);
        AuthenticateUserResponse adminAuth = await AuthenticateUser(adminEmail, ThePassword);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", adminAuth.AccessToken);

        HttpResponseMessage uploadResponse =
            await Client.PostAsJsonAsync(UploadEndpoint, BuildPremiumBody());
        uploadResponse.EnsureSuccessStatusCode();
        CreateRecordingResponse uploadEntity = await uploadResponse.ParseContentAsync<CreateRecordingResponse>();

        // Given: a regular user registers, logs in and has a cancelled (inactive) subscription
        string userEmail = TheFaker.Internet.Email();
        AuthenticateUserResponse userAuth = await RegisterConfirmAndLogin(TheFaker.Person.FullName, userEmail, ThePassword);

        JwtSecurityTokenHandler handler = new();
        JwtSecurityToken? jsonToken = handler.ReadJwtToken(userAuth.AccessToken);
        string userId = jsonToken.Subject;
        
        Result<SubscriptionEntity> seedResult =
            await SeedSubscriptionForRecordingTest(userId, SubscriptionStatus.Cancelled, OrderStatus.Cancelled);
        Assert.That(seedResult.IsSuccess, Is.True,
            $"Subscription seeding should succeed but got error: '{(seedResult.IsFailure ? seedResult.Error : "N/A")}'");

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", userAuth.AccessToken);

        // When: the user with an inactive subscription requests the download URL for the premium recording
        HttpResponseMessage response =
            await Client.GetAsync(BuildDownloadUrlEndpoint(uploadEntity.Id.ToString()));

        // Then: the response should be 400 BadRequest with OnlyForSubscribers error
        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.BadRequest),
            $"Expected 400 BadRequest for premium recording with inactive subscription but got {(int)response.StatusCode} {response.StatusCode}");

        BadRequestResponse entity = await response.ParseContentAsync<BadRequestResponse>();
        
        Assert.That(
            entity.ErrorCode,
            Is.EqualTo(RecordingErrors.OnlyForSubscribers),
            $"Expected error code '{RecordingErrors.OnlyForSubscribers}' (only for subscribers) but got: '{entity.ErrorCode}'");
    }

    [Test]
    [DisplayName("Should return 200 with non-empty URL when premium recording and user has active subscription")]
    public async Task ShouldReturn200WhenPremiumRecordingAndSubscriptionIsActive()
    {
        // Given: an admin uploads a premium recording
        string adminEmail = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Person.FullName, adminEmail, ThePassword);
        AuthenticateUserResponse adminAuth = await AuthenticateUser(adminEmail, ThePassword);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", adminAuth.AccessToken);

        HttpResponseMessage uploadResponse = await Client.PostAsJsonAsync(UploadEndpoint, BuildPremiumBody());
        uploadResponse.EnsureSuccessStatusCode();
        CreateRecordingResponse uploadEntity = await uploadResponse.ParseContentAsync<CreateRecordingResponse>();
        
        // And: a regular user registers, logs in and has an active subscription
        string userEmail = TheFaker.Internet.Email();
        AuthenticateUserResponse userAuth = await RegisterConfirmAndLogin(TheFaker.Person.FullName, userEmail, ThePassword);

        JwtSecurityTokenHandler handler = new();
        JwtSecurityToken? jsonToken = handler.ReadJwtToken(userAuth.AccessToken);
        string userId = jsonToken.Subject;
        Result<SubscriptionEntity> seedResult =
            await SeedSubscriptionForRecordingTest(userId, SubscriptionStatus.Active, OrderStatus.Paid);
        
        Assert.That(seedResult.IsSuccess, Is.True,
            $"Subscription seeding should succeed but got error: '{(seedResult.IsFailure ? seedResult.Error : "N/A")}'");

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", userAuth.AccessToken);

        // When: the user with an active subscription requests the download URL for the premium recording
        HttpResponseMessage response =
            await Client.GetAsync(BuildDownloadUrlEndpoint(uploadEntity.Id.ToString()));

        // Then: the response should be 200 OK with a non-empty download URL
        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.OK),
            $"Expected 200 OK for premium recording with active subscription but got {(int)response.StatusCode} {response.StatusCode}");

        GetRecordingDownloadUrlResponse urlEntity = await uploadResponse.ParseContentAsync<GetRecordingDownloadUrlResponse>();
        
        Assert.That(
            urlEntity.DownloadUrl,
            Is.Not.Null.And.Not.Empty,
            $"Expected a non-empty DownloadUrl for active subscriber but got: '{urlEntity.DownloadUrl}'");
    }
}
