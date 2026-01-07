using System.ComponentModel;
using System.Net;
using System.Net.Http.Json;
using Bogus;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.AuthenticateUsers;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.Auth;

[TestFixture]
public class AuthenticateTest : GenericAcceptanceTest<VibraHekaProgram>
{
    private const string DefaultPassword = "Password123@";

    [Test]
    public async Task ShouldAuthenticateAConfirmedUser()
    {
        // Given: A registered and confirmed user
      
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(TheFaker.Person.FullName, email, ThePassword);

        // When: The user is authenticated
        AuthenticateUserCommand command = new(email, ThePassword);
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/v1/auth/authenticate", command);

        // Then: Should return a JWT token
        ResponseEntity token = await response.GetAsResponseEntityAndContentAs<AuthenticationResult>();
        AuthenticationResult? result = token.GetContentAs<AuthenticationResult>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Role, Is.EqualTo(UserRole.User));
    }
    

    #region Validation Tests

    // === EMAIL VALIDATION TESTS ===
    [TestCase("", DefaultPassword, UserException.InvalidEmail)]
    [TestCase(null, DefaultPassword, UserException.InvalidEmail)]
    [TestCase("   ", DefaultPassword, UserException.InvalidEmail)]
    [TestCase("invalid-email", DefaultPassword, UserException.InvalidEmail)]

    // === PASSWORD VALIDATION TESTS ===
    [TestCase("test@example.com", "", UserException.InvalidPassword)]
    [TestCase("test@example.com", null, UserException.InvalidPassword)]
    [TestCase("test@example.com", "12345", UserException.InvalidPassword)] // Menos de 6 caracteres
    [DisplayName("Should not allow authentication with wrong data format")]
    public async Task ShouldNotAllowAuthenticationWithWrongData(string? email, string? password,
        string expectedErrorKeyword)
    {
        // Given: A command with invalid format data
        AuthenticateUserCommand command = new(email!, password!);

        // When: The client is invoked
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/v1/auth/authenticate", command);

        // Then: Should return BadRequest due to Validator
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        // And: Check error code
        ResponseEntity responseObject = await response.GetAsResponseEntityAndContentAs<AuthenticationResult>();
        Assert.That(responseObject.ErrorCode, Is.EqualTo(expectedErrorKeyword));
    }

    #endregion

    #region Business Logic Tests

    [Test]
    [DisplayName("Should return BadRequest when user is not confirmed")]
    public async Task ShouldReturnBadRequestWhenUserIsNotConfirmed()
    {
        // Given: A registered user but NOT confirmed
        Faker faker = new();
        string email = faker.Internet.Email();
        await RegisterUser(faker.Person.FullName, email, DefaultPassword);

        // When: Attempting to authenticate
        AuthenticateUserCommand authCommand = new(email, DefaultPassword);
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/v1/auth/authenticate", authCommand);

        // Then: Should return BadRequest (según el default del switch en tu AuthController para UserNotConfirmed)
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            "Should return BadRequest for unconfirmed users");

        // And: The error code should be UserNotConfirmed (E-003)
        ResponseEntity responseObject = await response.GetAsResponseEntity();
        Assert.That(responseObject.ErrorCode, Is.EqualTo(UserException.UserNotConfirmed));
    }


    [Test]
    [DisplayName("Should return NotFound when user does not exist")]
    public async Task ShouldReturnNotFoundWhenUserDoesNotExist()
    {
        // Given: A non-existent user
        AuthenticateUserCommand command = new("ghost@nonexistent.com", DefaultPassword);

        // When: Attempting to authenticate
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/v1/auth/authenticate", command);

        // Then: Should return NotFound 
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

        ResponseEntity responseObject = await response.GetAsResponseEntityAndContentAs<AuthenticationResult>();
        Assert.That(responseObject.ErrorCode, Is.EqualTo(UserException.UserNotFound));
    }

    [Test]
    [DisplayName("Should return NotFound when password is incorrect")]
    public async Task ShouldReturnNotFoundWhenPasswordIsIncorrect()
    {
        // Given: A registered user
        Faker faker = new();
        string email = faker.Internet.Email();
        await RegisterAndConfirmUser(faker.Person.FullName, email, DefaultPassword);

        // When: Authenticating with wrong password
        AuthenticateUserCommand command = new(email, "WrongPassword123!");
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/v1/auth/authenticate", command);

        // Then: Should return NotFound (según el switch en tu AuthController para InvalidPassword)
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

        ResponseEntity responseObject = await response.GetAsResponseEntity();
        Assert.That(responseObject.ErrorCode, Is.EqualTo(UserException.InvalidPassword));
    }

    #endregion
}
