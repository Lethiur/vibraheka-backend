using System.ComponentModel;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using MediatR;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Settings.Commands.ChangeTemplateForAction;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.Settings;

public class ChanteTemplateTest: GenericAcceptanceTest<VibraHekaProgram>
{
    [Test]
    [DisplayName("Should update verification email template successfully when user is admin")]
    public async Task ShouldUpdateVerificationEmailTemplateSuccessfullyWhenUserIsAdmin()
    {
        // Given: A registered and confirmed admin user
        string email = TheFaker.Internet.Email();
        string username = TheFaker.Person.FullName;
        const string templateID = "verification-email";
        await RegisterAndConfirmAdmin(username, email, ThePassword);
        
        // And: The user is authenticated
        var authResult = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
        
        // And: Template in the DB
        await SeedEmailTemplate(templateID, "test/verification-email.html");

        // And: A command to change the template
        var command = new ChangeTemplateForActionCommand(templateID, ActionType.UserVerification);

        // When: The admin attempts to change the template
        HttpResponseMessage response = await Client.PatchAsJsonAsync("api/v1/settings/ChangeTemplate", command);

        // Then: The response should be 200 OK
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Admin should be able to update the template");
        
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.Success, Is.True);
    }

    [Test]
    [DisplayName("Should return BadRequest when template is empty")]
    public async Task ShouldReturnBadRequestWhenTemplateIsEmpty()
    {
        // Given: An authenticated admin
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Person.FullName, email, ThePassword);
        var authResult = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

        // And: An invalid command (empty template)
        var command = new ChangeTemplateForActionCommand("", ActionType.UserVerification);

        // When: Sending the request
        HttpResponseMessage response = await Client.PatchAsJsonAsync("api/v1/settings/ChangeTemplate", command);

        // Then: Should return 400 BadRequest
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.Success, Is.False);
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(EmailTemplateErrors.InvalidTempalteID));
    }

    [Test]
    [DisplayName("Should return Unauthorized when no token is provided")]
    public async Task ShouldReturnUnauthorizedWhenNoTokenProvided()
    {
        // Given: A command but no authorization header
        var command = new ChangeTemplateForActionCommand("Some template", ActionType.PasswordReset);

        // When: Any user (or guest) attempts to change the template
        HttpResponseMessage response = await Client.PatchAsJsonAsync("api/v1/settings/ChangeTemplate", command);

        // Then: Should return 401 Unauthorized (o 403 dependiendo de la configuración de tu middleware)
        Assert.That(response.StatusCode, Is.AnyOf(HttpStatusCode.Unauthorized));
    }
    
    private async Task SeedEmailTemplate(string id, string subject)
    {
        
        IEmailTemplatesRepository repository = GetObjectFromFactory<IEmailTemplatesRepository>();

        EmailEntity template = new()
        {
            ID = id,
            Path = subject,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        Result<Unit> saveTemplate = await repository.SaveTemplate(template);
        Assert.That(saveTemplate.IsSuccess, Is.True);
    }
    
}
