using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NUnit.Framework;
using VibraHeka.Application.EmailTemplates.Commands.EditTemplateName;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Web.AcceptanceTests.Generic;
using VibraHeka.Web.Entities;

namespace VibraHeka.Web.AcceptanceTests.EmailTemplate;

[TestFixture]
public class EditTemplateNameTest : GenericAcceptanceTest<VibraHekaProgram>
{
    [Test]
    public async Task ShouldEditTemplateNameWhenUserIsAdmin()
    {
        // Given: An admin user and an existing template skeleton
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        
        string initialName = $"Initial-{TheFaker.Random.AlphaNumeric(8)}";
        HttpResponseMessage createResponse = await Client.PutAsync($"/api/v1/email-templates/create-skeleton?templateName={initialName}", null);
        ResponseEntity createEntity = await createResponse.GetAsResponseEntityAndContentAs<string>();
        string templateId = createEntity.GetContentAs<string>()!;

        // When: Changing the name
        string newName = $"Updated-{TheFaker.Random.AlphaNumeric(8)}";
        var editRequest = new { TemplateID = templateId, NewTemplateName = newName };
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/email-templates/change-name", editRequest);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Happy Path check: Verify name updated in the list
        HttpResponseMessage listResponse = await Client.GetAsync("/api/v1/email-templates");
        ResponseEntity listResponseEntity = await listResponse.GetAsResponseEntityAndContentAs<IEnumerable<EmailTemplateResponseDTO>>();
        IEnumerable<EmailTemplateResponseDTO>? templates = listResponseEntity.GetContentAs<IEnumerable<EmailTemplateResponseDTO>>();
        Assert.That(templates!.Any(t => t.TemplateID == templateId && t.TemplateName == newName), Is.True);
    }

    [Test]
    public async Task ShouldReturnUnauthorizedWhenEditingNameAsNonAdmin()
    {
        // Given: A non-admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: Changing a template name
        var editRequest = new { TemplateID = Guid.NewGuid().ToString(), NewTemplateName = "Unauthorized" };
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/email-templates/change-name", editRequest);

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

        // When: Changing name with invalid ID
        var editRequest = new { TemplateID = "not-a-guid", NewTemplateName = "Valid Name" };
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/email-templates/change-name", editRequest);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(EmailTemplateErrors.InvalidTempalteID));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenNewNameIsTooShort()
    {
        // Given: An admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: Changing name to a very short name
        var editRequest = new { TemplateID = Guid.NewGuid().ToString(), NewTemplateName = "Ab" };
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/email-templates/change-name", editRequest);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(EmailTemplateErrors.InvalidTemplateName));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenTemplateNotFound()
    {
        // Given: An admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: Changing name of a non-existent template
        var editRequest = new { TemplateID = Guid.NewGuid().ToString(), NewTemplateName = "Valid Name" };
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/email-templates/change-name", editRequest);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(EmailTemplateErrors.TemplateNotFound));
    }
}
