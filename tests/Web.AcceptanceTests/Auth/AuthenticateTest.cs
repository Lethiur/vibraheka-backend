using System.ComponentModel;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Web.Authentication;

namespace VibraHeka.Web.AcceptanceTests.Auth;

[TestFixture]
public class AuthenticateTest : GenericAuthAcceptanceTest
{
    [Test]
    public async Task ShouldAuthenticateAConfirmedUser()
    {
        // Given: A registered and confirmed user

        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(email, ThePassword);

        // When: The user is authenticated
        AuthenticateUserResponse token = await AuthenticateAsNewUser();

        // Then: Should return a JWT token
        Assert.That(token, Is.Not.Null);
        Assert.That(token.AccessToken, Is.Not.Null.And.Not.Empty);
        Assert.That(token.RefreshToken, Is.Not.Null.And.Not.Empty);
        Assert.That(token.Role, Is.EqualTo(UserRole.User));
    }


    #region Validation Tests

    // === EMAIL VALIDATION TESTS ===
    [TestCase("", ThePassword, UserErrors.InvalidEmail)]
    [TestCase(null, ThePassword, UserErrors.InvalidEmail)]
    [TestCase("   ", ThePassword, UserErrors.InvalidEmail)]
    [TestCase("invalid-email", ThePassword, UserErrors.InvalidEmail)]

    // === PASSWORD VALIDATION TESTS ===
    [TestCase("test@example.com", "", UserErrors.InvalidPassword)]
    [TestCase("test@example.com", null, UserErrors.InvalidPassword)]
    [TestCase("test@example.com", "12345", UserErrors.InvalidPassword)] // Menos de 6 caracteres
    [DisplayName("Should not allow authentication with wrong data format")]
    public async Task ShouldNotAllowAuthenticationWithWrongData(string? email, string? password,
        string expectedErrorKeyword)
    {
        // Given: A command with invalid format data
        AuthenticateUserRequest request = new() {Email = email!, Password = password!};
        
        // When: The client is invoked
        // Then: Should return BadRequest due to Validator
        await PerformCallAndExpectError(() => InvokeAuthenticateEndpoint(request), expectedErrorKeyword);
    }

    #endregion

    #region Business Logic Tests

    [Test]
    [DisplayName("Should return BadRequest when user is not confirmed")]
    public async Task ShouldReturnBadRequestWhenUserIsNotConfirmed()
    {
        // Given: A registered user but NOT confirmed
        string email = TheFaker.Internet.Email();
        await RegisterUser(email);

        // When: Attempting to authenticate
        // Then: Should return BadRequest (según el default del switch en tu AuthController para UserNotConfirmed)
        // And: The error code should be UserNotConfirmed (E-003)
        AuthenticateUserRequest authCommand = new() { Email = email, Password = ThePassword };
        await PerformCallAndExpectError(() => InvokeAuthenticateEndpoint(authCommand), UserErrors.UserNotConfirmed);
    }


    [Test]
    [DisplayName("Should return NotFound when user does not exist")]
    public async Task ShouldReturnNotFoundWhenUserDoesNotExist()
    {
        // Given: A non-existent user
        AuthenticateUserRequest command = new() { Email = "ghost@nonexistent.com", Password = ThePassword };

        // When: Attempting to authenticate
        // Then: Should return NotFound
        await PerformCallAndExpectError(() => InvokeAuthenticateEndpoint(command), UserErrors.UserNotFound);
    }

    [Test]
    [DisplayName("Should return NotFound when password is incorrect")]
    public async Task ShouldReturnNotFoundWhenPasswordIsIncorrect()
    {
        // Given: A registered user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(email);

        // When: Authenticating with wrong password
        AuthenticateUserRequest command = new() { Email = email, Password = "WrongPassword123!" };
        await PerformCallAndExpectError(() => InvokeAuthenticateEndpoint(command), UserErrors.NotAuthorized);
    }

    #endregion
}
