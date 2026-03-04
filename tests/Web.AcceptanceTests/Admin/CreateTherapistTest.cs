using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Domain.Models.Results.User;
using VibraHeka.Web.AcceptanceTests.Generic;

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
        AuthenticationResult authenticationResult = await AuthenticateUser(email, ThePassword);

        // And: Authorization header with user token
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);

        // When: Calling Create Therapist endpoint
        HttpResponseMessage postAsJsonAsync = await Client.PutAsJsonAsync("/api/v1/admin/addTherapist",
            CreateValidDTO(therapistEmail));

        // Then: Request is unauthorized
        Assert.That(postAsJsonAsync.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldReturn403IfNotAuthenticated()
    {
        // Given: No authentication token

        // When: Calling Create Therapist endpoint
        HttpResponseMessage postAsJsonAsync = await Client.PutAsJsonAsync("/api/v1/admin/addTherapist",
            CreateValidDTO());

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
        AuthenticationResult authenticationResult = await AuthenticateUser(email, ThePassword);

        // And: Authorization header with admin token
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);

        // When: Calling Create Therapist endpoint with valid payload
        HttpResponseMessage postAsJsonAsync = await Client.PutAsJsonAsync("/api/v1/admin/addTherapist",
            CreateValidDTO(TheFaker.Internet.Email(), "Valid Therapist", "middle name", "last name", "bio"));

        // Then: Response is OK
        Assert.That(postAsJsonAsync.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // And: API response marks operation as success
        ResponseEntity entity = await postAsJsonAsync.GetAsResponseEntityAndContentAs<string>();
        string? createdTherapistId = entity.GetContentAs<string>();
        Assert.That(entity.Success, Is.True);
        Assert.That(createdTherapistId, Is.Not.Null.And.Not.Empty);

        // And: The created therapist appears in admin listing with same id.
        HttpResponseMessage listResponse = await Client.GetAsync("/api/v1/admin/therapists");
        ResponseEntity listEntity = await listResponse.GetAsResponseEntityAndContentAs<IEnumerable<UserEntity>>();
        IEnumerable<UserEntity>? therapists = listEntity.GetContentAs<IEnumerable<UserEntity>>();
        Assert.That(therapists, Is.Not.Null);
        Assert.That(therapists!.Any(t => t.Id == createdTherapistId), Is.True);
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
        UserDTO command = CreateCommandWithOverride(targetField, invalidValue);

        // When: Calling Create Therapist endpoint
        HttpResponseMessage response = await Client.PutAsJsonAsync("/api/v1/admin/addTherapist",
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
        UserDTO command = CreateInvalidFormatCommand(scenario);

        // When: Calling Create Therapist endpoint
        HttpResponseMessage response = await Client.PutAsJsonAsync("/api/v1/admin/addTherapist",
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
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);

        // Then: Admin bearer token is set
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
    }

    private static async Task AssertBadRequestWithError(HttpResponseMessage response, string expectedErrorCode)
    {
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        ResponseEntity entity = await response.GetAsResponseEntity();
        Assert.That(entity.Success, Is.False);
        Assert.That(entity.ErrorCode, Does.Contain(expectedErrorCode));
    }

    private UserDTO CreateCommandWithOverride(string targetField, string? value)
    {
        UserDTO validDTO = CreateValidDTO();
        switch (targetField)
        {
            case "Email" : validDTO.Email = value!; break;
            case "FirstName": validDTO.FirstName = value!; break;
            case "MiddleName": validDTO.MiddleName = value!; break;
            case "LastName": validDTO.LastName = value!; break;
        }

        return validDTO;
    }

    private UserDTO CreateInvalidFormatCommand(string scenario)
    {
        return scenario switch
        {
            "BioTooLong" => CreateValidDTO(bio: new string('a', 1001)),
            "ProfilePictureUrlInvalid" => CreateValidDTO(profilePictureUrl: "ftp://invalid-url.com/image.png"),
            "PhoneInvalidFormat" => CreateValidDTO(phoneNumber: "abc-123"),
            "PhoneTooLong" => CreateValidDTO(phoneNumber: new string('1', 31)),
            "EmailTooLong" => CreateValidDTO(email: $"{new string('a', 315)}@example.com"),
            _ => CreateValidDTO()
        };
    }

    private UserDTO CreateValidDTO(
        string? email = null,
        string? firstName = null,
        string? middleName = null,
        string? lastName = null,
        string? bio = null,
        string? profilePictureUrl = null,
        string? phoneNumber = null)
    {
        return new UserDTO
        {
            Email = email ?? $"{Guid.NewGuid():N}@example.com",
            FirstName = firstName ?? "Valid Therapist",
            MiddleName = middleName ?? "Valid Middle",
            LastName = lastName ?? "Valid Last",
            Bio = bio ?? string.Empty,
            ProfilePictureUrl = profilePictureUrl ?? "https://example.com/avatar.png",
            PhoneNumber = phoneNumber ?? "+34911111222",
            TimezoneID = "Europe/Madrid"
        };
    }
}
