using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.AuthenticateUsers;
using VibraHeka.Application.Users.Commands.ChangeAuthenticatedPassword;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.Auth;

[TestFixture]
public class ChangeAuthenticatedPasswordAcceptanceTest : GenericAcceptanceTest<VibraHekaProgram>
{
    [Test]
    public async Task ShouldReturnUnauthorizedWhenChangingPasswordWithoutAuthentication()
    {
        // Given: a valid command without bearer token.
        ChangeAuthenticatedPasswordCommand command = new("Password123@", "NewPassword123@", "NewPassword123@");

        // When: calling the authenticated password change endpoint.
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/auth/change-password", command);

        // Then: request should be unauthorized.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldChangePasswordWhenUserIsAuthenticated()
    {
        // Given: a registered and confirmed user authenticated with bearer token.
        string email = TheFaker.Internet.Email();
        const string currentPassword = "Password123@";
        const string newPassword = "NewPassword123@";

        await RegisterAndConfirmUser(TheFaker.Person.FullName, email, currentPassword);
        AuthenticationResult authResult = await AuthenticateUser(email, currentPassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

        ChangeAuthenticatedPasswordCommand command = new(currentPassword, newPassword, newPassword);

        // When: requesting password change.
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/auth/change-password", command);

        // Then: endpoint should return success.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.Success, Is.True);

        // And: old password should fail while new password should authenticate successfully.
        AuthenticateUserCommand oldPasswordCommand = new(email, currentPassword);
        HttpResponseMessage oldPasswordResponse = await Client.PostAsJsonAsync("/api/v1/auth/authenticate", oldPasswordCommand);
        Assert.That(oldPasswordResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        ResponseEntity oldPasswordEntity = await oldPasswordResponse.GetAsResponseEntity();
        Assert.That(oldPasswordEntity.ErrorCode, Is.EqualTo(UserErrors.InvalidPassword));

        AuthenticateUserCommand newPasswordCommand = new(email, newPassword);
        HttpResponseMessage newPasswordResponse = await Client.PostAsJsonAsync("/api/v1/auth/authenticate", newPasswordCommand);
        Assert.That(newPasswordResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenNewPasswordConfirmationDoesNotMatch()
    {
        // Given: a registered and confirmed authenticated user with mismatching new passwords.
        string email = TheFaker.Internet.Email();
        const string currentPassword = "Password123@";
        await RegisterAndConfirmUser(TheFaker.Person.FullName, email, currentPassword);
        AuthenticationResult authResult = await AuthenticateUser(email, currentPassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

        ChangeAuthenticatedPasswordCommand command = new(currentPassword, "NewPassword123@", "DifferentPassword123@");

        // When: requesting password change.
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/auth/change-password", command);

        // Then: validator should reject request.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(UserErrors.InvalidPassword));
    }

    [Test]
    public async Task ShouldReturnUnauthorizedWhenCurrentPasswordIsWrong()
    {
        // Given: an authenticated user providing wrong current password.
        string email = TheFaker.Internet.Email();
        const string currentPassword = "Password123@";
        await RegisterAndConfirmUser(TheFaker.Person.FullName, email, currentPassword);
        AuthenticationResult authResult = await AuthenticateUser(email, currentPassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

        ChangeAuthenticatedPasswordCommand command = new("WrongCurrent123@", "NewPassword123@", "NewPassword123@");

        // When: requesting password change.
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/auth/change-password", command);

        // Then: Cognito should reject credentials and endpoint should return unauthorized.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(UserErrors.NotAuthorized));
    }
}
