using System.Net;
using Bogus;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.AcceptanceTests.Generic;

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
        await RegisterUser(faker.Person.FullName, email, "Password123@");
        

        // When: Resending the confirmation code
        HttpResponseMessage response = await Client.GetAsync($"/api/v1/auth/resend-confirmation-code?email={email}");

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        ResponseEntity responseEntity = await response.GetAsResponseEntityAndContentAs<string>();
        Assert.That(responseEntity.Success, Is.True);
        Assert.That(responseEntity.Content, Is.EqualTo("Confirmation code resent successfully"));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenRequestingVerificationCodeTwiceWithinCooldownWindow()
    {
        // Given: a registered but unconfirmed user that can request the first code.
        Faker faker = new();
        string email = faker.Internet.Email();
        await RegisterUser(faker.Person.FullName, email, "Password123@");

        HttpResponseMessage firstResponse =
            await Client.GetAsync($"/api/v1/auth/resend-confirmation-code?email={email}");
        Assert.That(firstResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // When: the same user requests another code immediately (within 1 minute cooldown).
        HttpResponseMessage secondResponse =
            await Client.GetAsync($"/api/v1/auth/resend-confirmation-code?email={email}");

        // Then: endpoint should reject by cooldown policy.
        Assert.That(secondResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        ResponseEntity secondEntity = await secondResponse.GetAsResponseEntity();
        Assert.That(secondEntity.Success, Is.False);
        Assert.That(secondEntity.ErrorCode, Is.EqualTo(UserErrors.NotAuthorized));
    }

    [Test]
    public async Task ShouldAllowResendAfterCooldownWindowHasElapsed()
    {
        // Given: a registered user that already consumed one resend and is now in cooldown.
        Faker faker = new();
        string email = faker.Internet.Email();
        await RegisterUser(faker.Person.FullName, email, "Password123@");

        HttpResponseMessage firstResponse =
            await Client.GetAsync($"/api/v1/auth/resend-confirmation-code?email={email}");
        Assert.That(firstResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        HttpResponseMessage blockedResponse =
            await Client.GetAsync($"/api/v1/auth/resend-confirmation-code?email={email}");
        Assert.That(blockedResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        IUserService userService = GetObjectFromFactory<IUserService>();
        IActionLogRepository actionLogRepository = GetObjectFromFactory<IActionLogRepository>();
        string userId = (await userService.GetUserID(email, CancellationToken.None)).Value;
        await actionLogRepository.SaveActionLog(new ActionLogEntity
        {
            ID = userId,
            Action = ActionType.RequestVerificationCode,
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(-2)
        }, CancellationToken.None);

        // When: requesting resend again after moving the action timestamp outside cooldown.
        HttpResponseMessage responseAfterCooldown =
            await Client.GetAsync($"/api/v1/auth/resend-confirmation-code?email={email}");

        // Then: endpoint should allow the action again.
        Assert.That(responseAfterCooldown.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        ResponseEntity responseEntity = await responseAfterCooldown.GetAsResponseEntityAndContentAs<string>();
        Assert.That(responseEntity.Success, Is.True);
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
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.Success, Is.False);
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
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(UserErrors.InvalidEmail));
    }
}
