using System.ComponentModel;
using System.Data;
using System.Net;
using System.Net.Http.Json;
using Bogus;
using Newtonsoft.Json;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.RegisterUser;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.Auth;

[TestFixture]
[DisplayName("Register user acceptance tests")]
public class RegisterAcceptanceTest : GenericAcceptanceTest<VibraHekaProgram>
{
    [Test]
    [DisplayName("Should register a new user")]
    public async Task ShouldRegisterANewUser()
    {
        // Given: A command
        string email = TheFaker.Internet.Email();
        RegisterUserCommand command = new(email, "Password123!", "John Doe", "TEST", "TEST","Europe/Madrid");
        // When: The client is invoked
        HttpResponseMessage postAsJsonAsync = await Client.PostAsJsonAsync("/api/v1/auth/register", command);

        // Then: HTTP and payload should represent a successful registration.
        Assert.That(postAsJsonAsync.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The status code should be OK");
        ResponseEntity responseEntity = await postAsJsonAsync.GetAsResponseEntityAndContentAs<UserRegistrationResult>();
        UserRegistrationResult? result = responseEntity.GetContentAs<UserRegistrationResult>();
        Assert.That(responseEntity.Success, Is.True);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.UserId, Is.Not.Null.And.Not.Empty);

        // And: the user should be retrievable from persistence by returned id.
        UserEntity persistedUser = await CheckForUser(result.UserId);
        Assert.That(persistedUser, Is.Not.Null);
        Assert.That(persistedUser.Email, Is.EqualTo(email));
    }
    
   // === EMAIL TESTS ===
    [TestCase("", "Password123@", "John Doe", UserErrors.InvalidEmail)] // Email vacÃ­o
    [TestCase(null, "Password123@", "John Doe", UserErrors.InvalidEmail)] // Email null
    [TestCase("   ", "Password123@", "John Doe", UserErrors.InvalidEmail)] // Email solo espacios
    [TestCase("invalid-email", "Password123@", "John Doe", UserErrors.InvalidEmail)] // Email formato invÃ¡lido
    [TestCase("@domain.com", "Password123@", "John Doe", UserErrors.InvalidEmail)] // Email sin parte local
    [TestCase("user@", "Password123@", "John Doe", UserErrors.InvalidEmail)] // Email sin dominio
    [TestCase("user.domain.com", "Password123@", "John Doe", UserErrors.InvalidEmail)] // Email sin @
    
    // === PASSWORD TESTS ===
    [TestCase("test@example.com", "", "John Doe", UserErrors.InvalidPassword)] // Password vacÃ­o
    [TestCase("test@example.com", null, "John Doe", UserErrors.InvalidPassword)] // Password null
    [TestCase("test@example.com", "   ", "John Doe", UserErrors.InvalidPassword)] // Password solo espacios
    [TestCase("test@example.com", "1", "John Doe", UserErrors.InvalidPassword)] // Password 1 char
    [TestCase("test@example.com", "12", "John Doe", UserErrors.InvalidPassword)] // Password 2 chars
    [TestCase("test@example.com", "123", "John Doe", UserErrors.InvalidPassword)] // Password 3 chars
    [TestCase("test@example.com", "1234", "John Doe", UserErrors.InvalidPassword)] // Password 4 chars
    [TestCase("test@example.com", "12345", "John Doe", UserErrors.InvalidPassword)] // Password 5 chars (lÃ­mite)
    
    // === FULLNAME TESTS ===
    [TestCase("test@example.com", "Password123@", "", UserErrors.InvalidFullName)] // FullName vacÃ­o
    [TestCase("test@example.com", "Password123@", null, UserErrors.InvalidFullName)] // FullName null
    [TestCase("test@example.com", "Password123@", "   ", UserErrors.InvalidFullName)] // FullName solo espacios
    [TestCase("test@example.com", "Password123@", "\t", UserErrors.InvalidFullName)] // FullName solo tab
    [TestCase("test@example.com", "Password123@", "\n", UserErrors.InvalidFullName)] // FullName solo salto de lÃ­nea
    [TestCase("test@example.com", "Password123@", "\r\n", UserErrors.InvalidFullName)] // FullName CRLF
    [TestCase("test@example.com", "Password123@", "A", UserErrors.InvalidFullName)] // FullName 1 char
    [TestCase("test@example.com", "Password123@", "AB", UserErrors.InvalidFullName)] // FullName 2 chars (lÃ­mite)
    [TestCase("test@example.com", "Password123@", "  A  ", UserErrors.InvalidFullName)] // FullName con espacios al inicio/final
    
    // === EDGE CASES COMBINADOS ===
    [TestCase(null, null, null, "US-006 | US-001 | US-007")] 
    [TestCase("", "", "", "US-006 | US-001 | US-007")]
    [TestCase("   ", "   ", "   ", "US-006 | US-001 | US-007")] 
    
    [DisplayName("Should not allow registration with wrong data")]
    public async Task ShouldNotAllowRegistrationWithWrongData(string email, string password, string fullName, string expectedErrorKeyword)
    {
        // Given: A command with invalid data
        RegisterUserCommand command = new(email, password, fullName, "TEST", "TEST","Europe/Madrid");

    
        // When: The client is invoked
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/v1/auth/register", command);

        // Then: Should return BadRequest
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "The status code should be BadRequest");
        
        // And: The response should contain the expected error message
        string responseContent = await response.Content.ReadAsStringAsync();

        ResponseEntity responseObject = JsonConvert.DeserializeObject<ResponseEntity>(responseContent) ?? throw new DataException("The response content could not be deserialized to a ResponseEntity object.");
        Assert.That(responseObject.Content, Is.Null,$"The response should contain the error keyword '{expectedErrorKeyword}'. Actual response: {responseContent}");
        Assert.That(responseObject.ErrorCode, Is.EqualTo(expectedErrorKeyword));
    }
    
    [Test]
    [DisplayName("Should not allow duplicate user registration")]
    public async Task ShouldNotAllowDuplicateUserRegistration()
    {
        // Given: A valid user command
        Faker faker = new();
        string? email = faker.Internet.Email();
        RegisterUserCommand firstCommand = new(email, "Password123@", "John Doe", "test","test", "Europe/Madrid");
        RegisterUserCommand duplicateCommand = new(email, "DifferentPassword456!", "Jane Smith", "test","test",  "Europe/Madrid");

        // When: We register the user for the first time
        HttpResponseMessage firstResponse = await Client.PostAsJsonAsync("/api/v1/auth/register", firstCommand);

        // Then: First registration should succeed
        Assert.That(firstResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "First registration should succeed");
        ResponseEntity firstResponseEntity = await firstResponse.GetAsResponseEntityAndContentAs<UserRegistrationResult>();
        Assert.That(firstResponseEntity.Success, Is.True);
        UserRegistrationResult? firstUser = firstResponseEntity.GetContentAs<UserRegistrationResult>();
        Assert.That(firstUser, Is.Not.Null);
        Assert.That(firstUser!.UserId, Is.Not.Null.And.Not.Empty);

        // When: We try to register the same email again
        HttpResponseMessage duplicateResponse = await Client.PostAsJsonAsync("/api/v1/auth/register", duplicateCommand);

        // Then: Second registration should fail
        Assert.That(duplicateResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Duplicate registration should fail");

        // And: The response should indicate it's a duplicate email error
        string responseContent = await duplicateResponse.Content.ReadAsStringAsync();

        ResponseEntity responseObject = JsonConvert.DeserializeObject<ResponseEntity>(responseContent) ?? throw new DataException("The response content could not be deserialized to a ResponseEntity object.");
        
        Assert.That(responseObject.Content, Is.Null,$"The response should contain the error keyword 'E-000'. Actual response: {responseContent}");
        Assert.That(responseObject.ErrorCode, Is.EqualTo(UserErrors.UserAlreadyExist));
    }
    
}
