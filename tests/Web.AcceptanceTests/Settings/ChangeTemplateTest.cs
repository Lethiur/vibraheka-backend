using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NUnit.Framework;
using VibraHeka.Application.Settings.Commands.ChangeTemplateForAction;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.Settings;

[TestFixture]
public class ChangeTemplateTest : GenericAcceptanceTest<VibraHekaProgram>
{
    [Test]
    public async Task ShouldUpdateVerificationEmailTemplateSuccessfullyWhenUserIsAdmin()
    {
        // Given: A registered and confirmed admin user
        string email = TheFaker.Internet.Email();
        string username = TheFaker.Person.FullName;
        string templateID = Guid.NewGuid().ToString();
        await RegisterAndConfirmAdmin(username, email, ThePassword);
        
        // And: The user is authenticated
        AuthenticationResult authResult = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
        
        // And: Template in the DB
        await SeedEmailTemplate(templateID, "test/verification-email.html");

        // And: A command to change the template
        ChangeTemplateForActionCommand command = new ChangeTemplateForActionCommand(templateID, ActionType.UserVerification);

        // When: The admin attempts to change the template
        HttpResponseMessage response = await Client.PatchAsJsonAsync("api/v1/settings/ChangeTemplate", command);

        // Then: The response should be 200 OK
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.Success, Is.True);

        // Happy Path check: Verify association in the all-templates list
        HttpResponseMessage listResponse = await Client.GetAsync("api/v1/settings/all-templates");
        ResponseEntity listResponseEntity = await listResponse.GetAsResponseEntityAndContentAs<IEnumerable<TemplateForActionEntity>>();
        IEnumerable<TemplateForActionEntity>? associations = listResponseEntity.GetContentAs<IEnumerable<TemplateForActionEntity>>();
        Assert.That(associations!.Any(a => a.ActionType == ActionType.UserVerification && a.TemplateID == templateID), Is.True);
    }

    [Test]
    public async Task ShouldReturnUnauthorizedWhenNonAdminAttemptsToChangeTemplate()
    {
        // Given: A registered and confirmed standard user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(TheFaker.Person.FullName, email, ThePassword);
        
        // And: The user is authenticated
        AuthenticationResult authResult = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
        
        // And: A command to change the template
        ChangeTemplateForActionCommand command = new ChangeTemplateForActionCommand(Guid.NewGuid().ToString(), ActionType.UserVerification);

        // When: The non-admin attempts to change the template
        HttpResponseMessage response = await Client.PatchAsJsonAsync("api/v1/settings/ChangeTemplate", command);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenTemplateIdIsInvalid()
    {
        // Given: An admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: Changing template with invalid ID
        ChangeTemplateForActionCommand command = new ChangeTemplateForActionCommand("invalid-guid", ActionType.UserVerification);
        HttpResponseMessage response = await Client.PatchAsJsonAsync("api/v1/settings/ChangeTemplate", command);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(EmailTemplateErrors.InvalidTempalteID));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenTemplateNotFound()
    {
        // Given: An admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: Changing template to a non-existent one
        ChangeTemplateForActionCommand command = new ChangeTemplateForActionCommand(Guid.NewGuid().ToString(), ActionType.UserVerification);
        HttpResponseMessage response = await Client.PatchAsJsonAsync("api/v1/settings/ChangeTemplate", command);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
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
}
