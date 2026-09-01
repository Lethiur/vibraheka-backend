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
public class ResendConfirmationCodeAcceptanceTest : GenericAcceptanceTest<VibraHekaProgram>
{
    [Test]
    public async Task ShouldResendConfirmationCodeWhenUserIsRegistered()
    {
        // Given: A registered but not confirmed user
        Faker faker = new();
        string email = faker.Internet.Email();
        await RegisterUser(email, "Password123@");


        // When: Resending the confirmation code
        HttpResponseMessage response = await Client.PostAsJsonAsync($"/api/v1/auth/resend-confirmation-code",
            new ResendConfirmationCodeRequest { Email = email });

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenRequestingVerificationCodeTwiceWithinCooldownWindow()
    {
        // Given: a registered but unconfirmed user that can request the first code.
        Faker faker = new();
        string email = faker.Internet.Email();
        await RegisterUser(email, "Password123@");

        
        HttpResponseMessage firstResponse = await Client.PostAsJsonAsync($"/api/v1/auth/resend-confirmation-code",
            new ResendConfirmationCodeRequest { Email = email });
        Assert.That(firstResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // When: the same user requests another code immediately (within 1 minute cooldown).
        HttpResponseMessage secondResponse =
            await Client.PostAsJsonAsync($"/api/v1/auth/resend-confirmation-code",
                new ResendConfirmationCodeRequest { Email = email });

        // Then: endpoint should reject by cooldown policy.
        Assert.That(secondResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        BadRequestResponse responseEntity = await secondResponse.ParseContentAsync<BadRequestResponse>();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(UserErrors.NotAuthorized));
    }

    [Test]
    public async Task ShouldAllowResendAfterCooldownWindowHasElapsed()
    {
        // Given: a registered user that already consumed one resend and is now in cooldown.
        Faker faker = new();
        string email = faker.Internet.Email();
        await RegisterUser(email, "Password123@");

        HttpResponseMessage firstResponse =
            await Client.PostAsJsonAsync($"/api/v1/auth/resend-confirmation-code",
                new ResendConfirmationCodeRequest { Email = email });
        Assert.That(firstResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        HttpResponseMessage blockedResponse =
            await Client.PostAsJsonAsync($"/api/v1/auth/resend-confirmation-code",
                new ResendConfirmationCodeRequest { Email = email });
        Assert.That(blockedResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

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
        HttpResponseMessage responseAfterCooldown =
            await Client.PostAsJsonAsync($"/api/v1/auth/resend-confirmation-code",
                new ResendConfirmationCodeRequest { Email = email });

        // Then: endpoint should allow the action again.
        Assert.That(responseAfterCooldown.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenEmailDoesNotExist()
    {
        // Given: An email that is not registered
        string email = "nonexistent@example.com";

        // When: Resending the confirmation code
        HttpResponseMessage response = await Client.GetAsync($"/api/v1/auth/resend-confirmation-code?email={email}");

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        BadRequestResponse responseEntity = await response.ParseContentAsync<BadRequestResponse>();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(UserErrors.UserNotFound));
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("not-an-email")]
    public async Task ShouldReturnBadRequestWhenEmailFormatIsInvalid(string invalidEmail)
    {
        // Given: an invalid email format in query string.

        // When: requesting resend confirmation code.
        HttpResponseMessage response =
            await Client.GetAsync($"/api/v1/auth/resend-confirmation-code?email={invalidEmail}");

        // Then: validator should map to invalid email error code.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        BadRequestResponse responseEntity = await response.ParseContentAsync<BadRequestResponse>();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(UserErrors.InvalidEmail));
    }
}
