using System.Net;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Web.Users;

namespace VibraHeka.Web.AcceptanceTests.Users;

[TestFixture]
public class CreateTherapistTest : GenericUserAcceptanceTest
{
    [Test]
    public async Task ShouldReturn403IfUserIsNotAdmin()
    {
        // And: Authenticated as non-admin
        await AuthenticateAsNewUser();

        // When: Calling Create Therapist endpoint
        // Then: There should be a bad request with error
        await PerformCallAndExpectStatusCode(() => InvokeCreateTherapistEndpoint(ValidCreateTherapistRequest()),
            HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task ShouldReturn403IfNotAuthenticated()
    {
        // Given: No authentication token
        RemoveAuthHeader();
        
        // When: Calling Create Therapist endpoint
        await PerformCallAndExpectStatusCode(() => InvokeCreateTherapistEndpoint(ValidCreateTherapistRequest()),
            HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task ShouldAddTherapistIfLoggedInAsAdmin()
    {
        // Given: Authenticated as admin
        await AuthenticateAsNewAdmin();

        // When: Calling Create Therapist endpoint with valid payload
        CreateTherapistResponse postAsJsonAsync = await PerformCreateTherapist(ValidCreateTherapistRequest());
        
        // Then: API response marks operation as success
        string createdTherapistId = postAsJsonAsync.Id.ToString();
        Assert.That(createdTherapistId, Is.Not.Null.And.Not.Empty);

        // And: The created therapist appears in admin listing with same id.
        IEnumerable<UserDTO> therapists = await PerformGetTherapists();
        Assert.That(therapists.Any(t => t.Id == Guid.Parse(createdTherapistId)), Is.True);
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
    [TestCase("AB", UserErrors.InvalidForm, "PhoneNumber")]
    public async Task ShouldReturnBadRequestWhenNameOrEmailIsInvalid(string? invalidValue, string expectedErrorCode,
        string targetField = "Email")
    {
        // Given: Authenticated as admin
        await AuthenticateAsNewAdmin();

        // And: Payload with invalid email/name field
        CreateTherapistRequest command = CreateCommandWithOverride(targetField, invalidValue);

        // When: Calling Create Therapist endpoint
        // Then: Response is BadRequest with expected validation error
        await PerformCallAndExpectError(() => InvokeCreateTherapistEndpoint(command), expectedErrorCode);
    }
    
    [TestCase("PhoneTooLong", UserErrors.InvalidForm)]
    [TestCase("EmailTooLong", UserErrors.EmailTooLong)]
    public async Task ShouldReturnBadRequestWhenFormatRulesAreBroken(string scenario, string expectedErrorCode)
    {
        // Given: Authenticated as admin
        await AuthenticateAsNewAdmin();

        // And: Payload with invalid format/length for the scenario
        CreateTherapistRequest command = CreateInvalidFormatCommand(scenario);

        // When: Calling Create Therapist endpoint
        // Then: Response is BadRequest with expected validation error
        await PerformCallAndExpectError(() => InvokeCreateTherapistEndpoint(command), expectedErrorCode);
    }

    private CreateTherapistRequest CreateCommandWithOverride(string targetField, string? value)
    {
        CreateTherapistRequest validDTO = ValidCreateTherapistRequest();
        switch (targetField)
        {
            case "Email": validDTO.Email = value!; break;
            case "FirstName": validDTO.FirstName = value!; break;
            case "MiddleName": validDTO.MiddleName = value!; break;
            case "LastName": validDTO.LastName = value!; break;
            case "PhoneNumber": validDTO.PhoneNumber = value!; break;
        }

        return validDTO;
    }

    private CreateTherapistRequest CreateInvalidFormatCommand(string scenario)
    {
        return scenario switch
        {
            "PhoneTooLong" => ValidCreateTherapistRequest(phoneNumber: new string('1', 31)),
            "EmailTooLong" => ValidCreateTherapistRequest(email: $"{new string('a', 315)}@example.com"),
            _ => ValidCreateTherapistRequest()
        };
    }
}
