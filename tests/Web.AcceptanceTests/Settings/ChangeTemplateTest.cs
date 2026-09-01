using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NUnit.Framework;
using VibraHeka.Application.Settings.Commands.ChangeTemplateForAction;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Web.AcceptanceTests.Generic;
using VibraHeka.Web.AcceptanceTests.Utils;
using VibraHeka.Web.Authentication;
using VibraHeka.Web.Settings.Controllers;
using BadRequestResponse = VibraHeka.Web.Settings.Controllers.BadRequestResponse;

namespace VibraHeka.Web.AcceptanceTests.Settings;

[TestFixture]
public class ChangeTemplateTest : GenericAcceptanceTest<VibraHekaProgram>
{
    [Test]
    public async Task ShouldReturnUnauthorizedWhenNoAuthenticationIsProvided()
    {
        // Given: no authentication headers and a valid command payload.
        Client.DefaultRequestHeaders.Remove("Authorization");
        UpdateTemplateForActionRequest command = new() { TemplateId = Guid.Empty, ActionType = ActionType.UserVerification };
        
        // When: calling change-template endpoint without auth.
        HttpResponseMessage response = await Client.PatchAsJsonAsync("api/v1/settings/ChangeTemplate", command);

        // Then: authorization middleware rejects the request.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [TestCase(ActionType.UserRegistered, "test/welcome-email.html")]
    [TestCase(ActionType.SubscriptionThankYou, "test/subscription-thank-you-email.html")]
    [TestCase(ActionType.TrialEndingSoon, "test/trial-ending-soon-email.html")]
    [TestCase(ActionType.PasswordChanged, "test/password-changed-email.html")]
    [TestCase(ActionType.SubscriptionCancelled, "test/password-changed-email.html")]
    [TestCase(ActionType.SubscriptionReactivated, "test/password-changed-email.html")]
    [TestCase(ActionType.ForgotPasswordCompleted, "test/password-changed-email.html")]
    [TestCase(ActionType.UserVerification, "test/user-verification-email.html")]
    [TestCase(ActionType.PasswordReset, "test/password-reset-email.html")]
    public async Task ShouldUpdateNewAdminManagedTemplatesSuccessfullyWhenUserIsAdmin(ActionType actionType, string templatePath)
    {
        // Given: a registered and authenticated admin user.
        string email = TheFaker.Internet.Email();
        string username = TheFaker.Person.FullName;
        Guid templateID = Guid.NewGuid();
        await RegisterAndConfirmAdmin(username, email, ThePassword);
        AuthenticateUserResponse authResult = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

        // And: a template entity exists in storage repository.
        await SeedEmailTemplate(templateID.ToString(), templatePath);

        // When: changing association for the requested action type.
        UpdateTemplateForActionRequest command = new() { TemplateId = templateID, ActionType = actionType };
        HttpResponseMessage response = await Client.PatchAsJsonAsync("api/v1/settings/ChangeTemplate", command);

        // Then: endpoint should accept and persist the association.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        
        bool foundAssociation = await WaitForTemplateAssociation(actionType, templateID, TimeSpan.FromSeconds(10));
        Assert.That(foundAssociation, Is.True);
    }

    [Test]
    public async Task ShouldReturnUnauthorizedWhenNonAdminAttemptsToChangeTemplate()
    {
        // Given: A registered and confirmed standard user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(TheFaker.Person.FullName, email, ThePassword);

        // And: The user is authenticated
        AuthenticateUserResponse authResult = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

        // And: A command to change the template
        UpdateTemplateForActionRequest command = new() { TemplateId = Guid.NewGuid(), ActionType = ActionType.UserVerification };

        // When: The non-admin attempts to change the template
        HttpResponseMessage response = await Client.PatchAsJsonAsync("api/v1/settings/admin/ChangeTemplate", command);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenTemplateIdIsInvalid()
    {
        // Given: An admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: Changing template with invalid ID
        UpdateTemplateForActionRequest command = new() { TemplateId = Guid.Empty, ActionType = ActionType.UserVerification };
        HttpResponseMessage response = await Client.PatchAsJsonAsync("api/v1/settings/admin/ChangeTemplate", command);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        BadRequestResponse responseEntity = await response.ParseContentAsync<BadRequestResponse>();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(EmailTemplateErrors.InvalidTempalteID));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenTemplateNotFound()
    {
        // Given: An admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: Changing template to a non-existent one
        UpdateTemplateForActionRequest command = new() { TemplateId = Guid.NewGuid(), ActionType = ActionType.UserVerification };
        HttpResponseMessage response = await Client.PatchAsJsonAsync("api/v1/settings/admin/ChangeTemplate", command);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        BadRequestResponse responseEntity = await response.ParseContentAsync<BadRequestResponse>();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(EmailTemplateErrors.TemplateNotFound));
    }

    private async Task SeedEmailTemplate(string id, string subject)
    {
        IEmailTemplatesRepository repository = GetObjectFromFactory<IEmailTemplatesRepository>();

        EmailEntity template = new()
        {
            ID = id,
            Path = subject,
            Name = "Verification Template",
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        await repository.SaveTemplate(template, CancellationToken.None);
    }

    private async Task<bool> WaitForTemplateAssociation(ActionType actionType, Guid templateId, TimeSpan timeout)
    {
        DateTime deadline = DateTime.UtcNow.Add(timeout);
        while (DateTime.UtcNow < deadline)
        {
            HttpResponseMessage listResponse = await Client.GetAsync("api/v1/settings/all-templates");
            GetTemplateResponse listResponseEntity =
                await listResponse.ParseContentAsync<GetTemplateResponse>();
            IEnumerable<TemplateForActionDTO> associations = listResponseEntity.TemplateList;

            if (associations.Any(a => a.ActionType == actionType && a.Id == templateId))
            {
                return true;
            }

            await Task.Delay(300);
        }

        return false;
    }
}
