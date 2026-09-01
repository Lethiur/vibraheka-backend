using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.AcceptanceTests.Generic;
using VibraHeka.Web.AcceptanceTests.Utils;
using VibraHeka.Web.Authentication;
using VibraHeka.Web.Users;
using BadRequestResponse = VibraHeka.Web.Users.BadRequestResponse;

namespace VibraHeka.Web.AcceptanceTests.Admin;

[TestFixture]
public class CreateTherapistTest : GenericAcceptanceTest<VibraHekaProgram>
{
    [Test]
    public async Task ShouldReturn403IfUserIsNotAdmin()
    {
        // Given: Registered user
        string email = TheFaker.Internet.Email();
        string therapistEmail = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(TheFaker.Person.FullName, email, ThePassword);

        // And: Authenticated as non-admin
        AuthenticateUserResponse authenticationResult = await AuthenticateUser(email, ThePassword);

        // And: Authorization header with user token
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);

        // When: Calling Create Therapist endpoint
        HttpResponseMessage postAsJsonAsync = await Client.PutAsJsonAsync("/api/v1/admin/addTherapist",
            CreateValidRequest(therapistEmail));

        // Then: Request is unauthorized
        Assert.That(postAsJsonAsync.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldReturn403IfNotAuthenticated()
    {
        // Given: No authentication token
        Client.DefaultRequestHeaders.Remove("Authorization");

        // When: Calling Create Therapist endpoint
        HttpResponseMessage postAsJsonAsync = await Client.PutAsJsonAsync("/api/v1/users/admin/create-therapist",
            CreateValidRequest());

        // Then: Request is unauthorized
        Assert.That(postAsJsonAsync.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldAddTherapistIfLoggedInAsAdmin()
    {
        // Given: Registered admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Person.FullName, email, ThePassword);

        // And: Authenticated as admin
        AuthenticateUserResponse authenticationResult = await AuthenticateUser(email, ThePassword);

        // And: Authorization header with admin token
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);

        // When: Calling Create Therapist endpoint with valid payload
        HttpResponseMessage postAsJsonAsync = await Client.PutAsJsonAsync("/api/v1/users/admin/create-therapist",
            CreateValidRequest(TheFaker.Internet.Email(), "Valid Therapist", "middle name", "last name", "bio"));

        // Then: Response is OK
        Assert.That(postAsJsonAsync.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // And: API response marks operation as success
        string createdTherapistId = await postAsJsonAsync.Content.ReadAsStringAsync();
        Assert.That(createdTherapistId, Is.Not.Null.And.Not.Empty);

        // And: The created therapist appears in admin listing with same id.
        HttpResponseMessage listResponse = await Client.GetAsync("/api/v1/users/admin/therapists");
        
        IEnumerable<UserEntity> therapists = await listResponse.ParseContentAsync<IEnumerable<UserEntity>>();
        Assert.That(therapists.Any(t => t.Id == createdTherapistId), Is.True);
    }

    [TestCase(null, UserErrors.InvalidEmail)]
    [TestCase("", UserErrors.InvalidEmail)]
    [TestCase("   ", UserErrors.InvalidEmail)]
    [TestCase("invalid-email", UserErrors.InvalidEmail)]
    [TestCase("AB", UserErrors.InvalidFullName, "FirstName")]
    [TestCase(null, UserErrors.InvalidFullName, "FirstName")]
    [TestCase("", UserErrors.InvalidFullName, "FirstName")]
    [TestCase("   ", UserErrors.InvalidFullName, "FirstName")]
    [TestCase("AB", UserErrors.InvalidFullName, "MiddleName")]
    [TestCase("AB", UserErrors.InvalidFullName, "LastName")]
    public async Task ShouldReturnBadRequestWhenNameOrEmailIsInvalid(string? invalidValue, string expectedErrorCode,
        string targetField = "Email")
    {
        // Given: Authenticated as admin
        await AuthenticateAsAdmin();

        // And: Payload with invalid email/name field
        CreateTherapistRequest command = CreateCommandWithOverride(targetField, invalidValue);

        // When: Calling Create Therapist endpoint
        HttpResponseMessage response = await Client.PutAsJsonAsync("/api/v1/users/admin/create-therapist",
            command);

        // Then: Response is BadRequest with expected validation error
        await AssertBadRequestWithError(response, expectedErrorCode);
    }

    [TestCase("BioTooLong", UserErrors.InvalidForm)]
    [TestCase("ProfilePictureUrlInvalid", UserErrors.InvalidForm)]
    [TestCase("PhoneInvalidFormat", UserErrors.InvalidForm)]
    [TestCase("PhoneTooLong", UserErrors.InvalidForm)]
    [TestCase("EmailTooLong", UserErrors.EmailTooLong)]
    public async Task ShouldReturnBadRequestWhenFormatRulesAreBroken(string scenario, string expectedErrorCode)
    {
        // Given: Authenticated as admin
        await AuthenticateAsAdmin();

        // And: Payload with invalid format/length for the scenario
        CreateTherapistRequest command = CreateInvalidFormatCommand(scenario);

        // When: Calling Create Therapist endpoint
        HttpResponseMessage response = await Client.PutAsJsonAsync("/api/v1/users/admin/create-therapist",
            command);

        // Then: Response is BadRequest with expected validation error
        await AssertBadRequestWithError(response, expectedErrorCode);
    }

    private async Task AuthenticateAsAdmin()
    {
        // Given: Admin account
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Person.FullName, email, ThePassword);

        // When: Authenticating as admin
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);

        // Then: Admin bearer token is set
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
    }

    private static async Task AssertBadRequestWithError(HttpResponseMessage response, string expectedErrorCode)
    {
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        BadRequestResponse entity = await response.ParseContentAsync<BadRequestResponse>();
        Assert.That(entity.ErrorCode, Does.Contain(expectedErrorCode));
    }

    private CreateTherapistRequest CreateCommandWithOverride(string targetField, string? value)
    {
        CreateTherapistRequest validDTO = CreateValidRequest();
        switch (targetField)
        {
            case "Email": validDTO.Email = value!; break;
            case "FirstName": validDTO.FirstName = value!; break;
            case "MiddleName": validDTO.MiddleName = value!; break;
            case "LastName": validDTO.LastName = value!; break;
        }

        return validDTO;
    }

    private CreateTherapistRequest CreateInvalidFormatCommand(string scenario)
    {
        return scenario switch
        {
            "PhoneInvalidFormat" => CreateValidRequest(phoneNumber: "abc-123"),
            "PhoneTooLong" => CreateValidRequest(phoneNumber: new string('1', 31)),
            "EmailTooLong" => CreateValidRequest(email: $"{new string('a', 315)}@example.com"),
            _ => CreateValidRequest()
        };
    }

    private CreateTherapistRequest CreateValidRequest(
        string? email = null,
        string? firstName = null,
        string? middleName = null,
        string? lastName = null,
        string? phoneNumber = null)
    {
        return new CreateTherapistRequest
        {
            Email = email ?? $"{Guid.NewGuid():N}@example.com",
            FirstName = firstName ?? "Valid Therapist",
            MiddleName = middleName ?? "Valid Middle",
            LastName = lastName ?? "Valid Last",
            PhoneNumber = phoneNumber ?? "+34911111222",
            TimezoneID = "Europe/Madrid"
        };
    }
}
