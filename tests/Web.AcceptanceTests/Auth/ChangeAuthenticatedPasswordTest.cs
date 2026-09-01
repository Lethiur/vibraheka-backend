using System.Net;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Web.Authentication;

namespace VibraHeka.Web.AcceptanceTests.Auth;

[TestFixture]
public class ChangeAuthenticatedPasswordTest : GenericAuthAcceptanceTest
{
    [Test]
    public async Task ShouldReturnUnauthorizedWhenChangingPasswordWithoutAuthentication()
    {
        // Given: a valid command without bearer token.
        RemoveAuthHeader();
        ChangePasswordRequest command = BuildChangePasswordRequestCorrectly();

        // When: calling the authenticated password change endpoint.
        // Then: request should be unauthorized.
        await PerformCallAndExpectStatusCode(() => InvokeChangePasswordEndpoint(command), HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task ShouldChangePasswordWhenUserIsAuthenticated()
    {
        // Given: a registered and confirmed user authenticated with bearer token.
        string email = TheFaker.Internet.Email();
        await RegisterConfirmAndLogin(email);

        ChangePasswordRequest command = BuildChangePasswordRequestCorrectly();

        // When: requesting password change.
        await PerformChangePassword(command);

        // And: old password should fail while new password should authenticate successfully.
        AuthenticateUserRequest oldPasswordCommand = BuildWithRegularPassword(ref email);
        await PerformCallAndExpectError(() => InvokeAuthenticateEndpoint(oldPasswordCommand), UserErrors.NotAuthorized);

        // And: Login with new password should succeed
        await PerformCallAndExpectStatusCode(
            () => InvokeAuthenticateEndpoint(BuildAuthenticateUserRequest(ref email, NewPassword)), HttpStatusCode.OK);
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenNewPasswordConfirmationDoesNotMatch()
    {
        // Given: a registered and confirmed authenticated user with mismatching new passwords.
        string email = TheFaker.Internet.Email();
        await RegisterConfirmAndLogin(email);

        ChangePasswordRequest command = new()
        {
            CurrentPassword = ThePassword,
            NewPassword = "NewPassword123@",
            NewPasswordConfirmation = "DifferentPassword123@"
        };

        // When: requesting password change.
        // Then: validator should reject request.
        await PerformCallAndExpectError(() => InvokeChangePasswordEndpoint(command), UserErrors.InvalidPassword);
    }

    [Test]
    public async Task ShouldReturnUnauthorizedWhenCurrentPasswordIsWrong()
    {
        // Given: an authenticated user providing wrong current password.
        string email = TheFaker.Internet.Email();
        await RegisterConfirmAndLogin(email);


        ChangePasswordRequest command = new()
        {
            CurrentPassword = "WrongCurrent123@",
            NewPassword = "NewPassword123@",
            NewPasswordConfirmation = "NewPassword123@"
        };

        // When: requesting password change.
        await PerformCallAndExpectError(() => InvokeChangePasswordEndpoint(command), UserErrors.NotAuthorized);
    }
}
