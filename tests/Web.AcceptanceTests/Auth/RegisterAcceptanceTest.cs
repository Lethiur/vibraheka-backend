using System.ComponentModel;
using System.Net;
using System.Net.Http.Json;
using Bogus;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.RegisterUser;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.AcceptanceTests.Generic;
using VibraHeka.Web.AcceptanceTests.Utils;
using VibraHeka.Web.Authentication;

namespace VibraHeka.Web.AcceptanceTests.Auth;

[TestFixture]
[DisplayName("Register user acceptance tests")]
public class RegisterAcceptanceTest : GenericAuthAcceptanceTest
{
    [Test]
    [DisplayName("Should register a new user")]
    public async Task ShouldRegisterANewUser()
    {
        // Given: A command
        string email = TheFaker.Internet.Email();
        RegisterUserRequest request = new()
        {
            Email = email,
            Password = "Password123!",
            FirstName = "TEST",
            MiddleName = "TEST",
            TimezoneID = "Europe/Madrid"
        };
        // When: The client is invoked
        RegisterUserResponse responseEntity = await PerformCallAndRetrieveContent<RegisterUserResponse>(() => InvokeRegistrationEndpoint(request));
        
        // Then: the user should be retrievable from persistence by returned id.
        UserEntity persistedUser = await CheckForUser(responseEntity.UserId);
        Assert.That(responseEntity, Is.Not.Null);
        Assert.That(responseEntity.UserId, Is.Not.Null.And.Not.Empty);
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
    [TestCase("test@example.com", "Password123@", "  A  ",
        UserErrors.InvalidFullName)] 

    // === EDGE CASES COMBINADOS ===
    [TestCase(null, null, null, "US-006 | US-001 | US-007")]
    [TestCase("", "", "", "US-006 | US-001 | US-007")]
    [TestCase("   ", "   ", "   ", "US-006 | US-001 | US-007")]
    [DisplayName("Should not allow registration with wrong data")]
    public async Task ShouldNotAllowRegistrationWithWrongData(string email, string password, string fullName,
        string expectedErrorKeyword)
    {
        // Given: A command with invalid data
        RegisterUserRequest command = new()
        {
            Email = email,
            Password = password,
            FirstName = fullName,
            MiddleName = "TEST",
            LastName = "TEST",
            TimezoneID = "Europe/Madrid"
        };
        
        // When: The client is invoked
        // Then: Should return BadRequest
        await PerformCallAndExpectError(() => InvokeRegistrationEndpoint(command), expectedErrorKeyword);
    }

    [Test]
    [DisplayName("Should not allow duplicate user registration")]
    public async Task ShouldNotAllowDuplicateUserRegistration()
    {
        // Given: A valid user command
        Faker faker = new();
        string? email = faker.Internet.Email();
        RegisterUserRequest firstCommand = new()
        {
            Email = email,
            Password = ThePassword,
            FirstName = "test",
            MiddleName = "TEST",
            LastName = "TEST",
            TimezoneID = "Europe/Madrid"
        };
        
        // When: We register the user for the first time
        // Then: First registration should succeed
        await PerformRegistration(firstCommand);
        
        // When: We try to register the same email again
        // And: The response should indicate it's a duplicate email error
        await PerformCallAndExpectError(() => InvokeRegistrationEndpoint(firstCommand), UserErrors.UserAlreadyExist);
    }
}
