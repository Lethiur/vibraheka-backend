using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NUnit.Framework;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Web.EmailTemplates;
using VibraHeka.Web.AcceptanceTests.Generic;
using VibraHeka.Web.AcceptanceTests.Utils;
using VibraHeka.Web.Authentication;
using BadRequestResponse = VibraHeka.Web.EmailTemplates.BadRequestResponse;

namespace VibraHeka.Web.AcceptanceTests.EmailTemplate;

[TestFixture]
public class EditTemplateNameTest : GenericAcceptanceTest<VibraHekaProgram>
{
    [Test]
    public async Task ShouldReturnUnauthorizedWhenEditingNameWithoutAuthentication()
    {
        // Given: no authenticated admin context.
        UpdateEmailTemplateNameRequest editRequest = new()
        {
            TemplateID = Guid.NewGuid(),
            Name = "NoAuthName"
        };

        // When: calling change-name endpoint.
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/email-templates", editRequest);

        // Then: endpoint returns unauthorized.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldEditTemplateNameWhenUserIsAdmin()
    {
        // Given: An admin user and an existing template skeleton
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        string initialName = $"Initial-{TheFaker.Random.AlphaNumeric(8)}";
        CreateEmailTemplateRequest createRequest = new()
        {
            Name = initialName
        };
        HttpResponseMessage createResponse = await Client.PutAsJsonAsync("/api/v1/email-templates", createRequest);
        CreateEmailTemplateResponse createEntity = await createResponse.ParseContentAsync<CreateEmailTemplateResponse>();
        Guid templateId = createEntity.TemplateId;

        // When: Changing the name
        string newName = $"Updated-{TheFaker.Random.AlphaNumeric(8)}";
        UpdateEmailTemplateNameRequest editRequest = new()
        {
            TemplateID = templateId,
            Name = newName
        };
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/email-templates", editRequest);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));


        // Happy Path check: Verify name updated in the list
        HttpResponseMessage listResponse = await Client.GetAsync("/api/v1/email-templates");
        GetTemplatesResponse listResponseEntity = await listResponse.ParseContentAsync<GetTemplatesResponse>();
        IEnumerable<SimpleEmailTemplateDTO> templates = listResponseEntity.Templates;
        Assert.That(templates.Any(t => t.ID == templateId && t.Name == newName), Is.True);
    }

    [Test]
    public async Task ShouldReturnUnauthorizedWhenEditingNameAsNonAdmin()
    {
        // Given: A non-admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: Changing a template name
        UpdateEmailTemplateNameRequest editRequest = new()
        {
            TemplateID = Guid.NewGuid(),
            Name = "Unauthorized"
        };
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/email-templates", editRequest);

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

        // When: Changing name with invalid ID
        using StringContent editRequest = new("{\"templateID\":\"not-a-guid\",\"name\":\"Valid Name\"}", System.Text.Encoding.UTF8, "application/json");
        HttpRequestMessage requestMessage = new(HttpMethod.Patch, "/api/v1/email-templates")
        {
            Content = editRequest
        };
        HttpResponseMessage response = await Client.SendAsync(requestMessage);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        BadRequestResponse responseEntity = await response.ParseContentAsync<BadRequestResponse>();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(EmailTemplateErrors.InvalidTempalteID));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenNewNameIsTooShort()
    {
        // Given: An admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: Changing name to a very short name
        UpdateEmailTemplateNameRequest editRequest = new()
        {
            TemplateID = Guid.NewGuid(),
            Name = "Ab"
        };
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/email-templates", editRequest);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        BadRequestResponse responseEntity = await response.ParseContentAsync<BadRequestResponse>();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(EmailTemplateErrors.InvalidTemplateName));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenTemplateNotFound()
    {
        // Given: An admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: Changing name of a non-existent template
        UpdateEmailTemplateNameRequest editRequest = new()
        {
            TemplateID = Guid.NewGuid(),
            Name = "Valid Name"
        };
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/email-templates", editRequest);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        BadRequestResponse responseEntity = await response.ParseContentAsync<BadRequestResponse>();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(EmailTemplateErrors.TemplateNotFound));
    }
}
