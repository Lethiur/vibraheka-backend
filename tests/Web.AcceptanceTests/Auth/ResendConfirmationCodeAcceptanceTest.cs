using System.Net;
using System.Net.Http.Json;
using Bogus;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.AcceptanceTests.Generic;
using VibraHeka.Web.AcceptanceTests.Utils;
using VibraHeka.Web.Authentication;

namespace VibraHeka.Web.AcceptanceTests.Auth;

[TestFixture]
public class ResendConfirmationCodeAcceptanceTest : GenericAuthAcceptanceTest
{
    [Test]
    public async Task ShouldResendConfirmationCodeWhenUserIsRegistered()
    {
        // Given: A registered but not confirmed user\
        string email = TheFaker.Internet.Email();
        await RegisterUser(email);

        // When: Resending the confirmation code
        // Then
        await PerformResendConfirmationCode(new ResendConfirmationCodeRequest { Email = email });
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenRequestingVerificationCodeTwiceWithinCooldownWindow()
    {
        // Given: a registered but unconfirmed user that can request the first code.

        string email = TheFaker.Internet.Email();
        await RegisterUser(email);

        await PerformResendConfirmationCode(new ResendConfirmationCodeRequest { Email = email });


        // When: the same user requests another code immediately (within 1 minute cooldown).
        // Then: endpoint should reject by cooldown policy.
        await PerformCallAndExpectError(
            () => InvokeResendConfirmationCodeEndpoint(new ResendConfirmationCodeRequest { Email = email }),
            UserErrors.NotAuthorized);
    }

    [Test]
    public async Task ShouldAllowResendAfterCooldownWindowHasElapsed()
    {
        // Given: a registered user that already consumed one resend and is now in cooldown.
        string email = TheFaker.Internet.Email();
        await RegisterUser(email);
        ResendConfirmationCodeRequest request = new ResendConfirmationCodeRequest { Email = email };
        await PerformResendConfirmationCode(request);

        await PerformCallAndExpectStatusCode(
            () => InvokeResendConfirmationCodeEndpoint(request),
            HttpStatusCode.BadRequest);
        
        IUserService userService = GetObjectFromFactory<IUserService>();
        IActionLogRepository actionLogRepository = GetObjectFromFactory<IActionLogRepository>();
        string userId = (await userService.GetUserID(email, CancellationToken.None)).Value;
        await actionLogRepository.SaveActionLog(
            new ActionLogEntity
            {
                ID = userId,
                Action = ActionType.RequestVerificationCode,
                Timestamp = DateTimeOffset.UtcNow.AddMinutes(-2)
            }, CancellationToken.None);

        // When: requesting resend again after moving the action timestamp outside cooldown.
        await PerformCallAndExpectStatusCode(
            () => InvokeResendConfirmationCodeEndpoint(request),
            HttpStatusCode.NoContent);

    }

    [Test]
    public async Task ShouldReturnBadRequestWhenEmailDoesNotExist()
    {
        // Given: An email that is not registered
        string email = "nonexistent@example.com";

        // When: Resending the confirmation code
        await PerformCallAndExpectError(
            () => InvokeResendConfirmationCodeEndpoint(new ResendConfirmationCodeRequest { Email = email }),
            UserErrors.UserNotFound);
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("not-an-email")]
    public async Task ShouldReturnBadRequestWhenEmailFormatIsInvalid(string invalidEmail)
    {
        // Given: an invalid email format in query string.
        // When: requesting resend confirmation code.
        // Then: endpoint should reject with invalid email error.
        await PerformCallAndExpectError(
            () => InvokeResendConfirmationCodeEndpoint(new ResendConfirmationCodeRequest { Email = invalidEmail }),
            UserErrors.InvalidEmail);
    }
}
