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
public class ChangeTemplateContentsTest : GenericAcceptanceTest<VibraHekaProgram>
{
    [Test]
    public async Task ShouldReturnUnauthorizedWhenChangingContentsWithoutAuthentication()
    {
        // Given: a valid request payload but no authentication token.
        Client.DefaultRequestHeaders.Remove("Authorization");
        using MultipartFormDataContent form = new();
        form.Add(new StringContent(Guid.NewGuid().ToString()), "templateID");
        form.Add(new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("content"))), "templateFile", "template.html");

        // When: calling the change-contents endpoint.
        HttpResponseMessage response = await Client.PutAsync("/api/v1/email-templates/content", form);

        // Then: endpoint should reject unauthenticated access.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldChangeTemplateContentsWhenUserIsAdmin()
    {
        // Given: An admin user and an existing template skeleton
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        string initialName = $"ContentTest-{TheFaker.Random.AlphaNumeric(8)}";
        CreateEmailTemplateRequest createRequest = new()
        {
            Name = initialName
        };
        HttpResponseMessage createResponse = await Client.PutAsJsonAsync("/api/v1/email-templates", createRequest);
        CreateEmailTemplateResponse createEntity = await createResponse.ParseContentAsync<CreateEmailTemplateResponse>();
        Guid templateId = createEntity.TemplateId;

        // And: A new content file
        string newContent = "<html><body>Updated Content</body></html>";
        using MultipartFormDataContent form = new();
        form.Add(new StringContent(templateId.ToString()), "templateID");
        form.Add(new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(newContent))), "templateFile", "template.html");

        // When: Changing template contents
        HttpResponseMessage response = await Client.PutAsync("/api/v1/email-templates/content", form);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Happy Path check: Verify the download URL can be retrieved after updating the content.
        HttpResponseMessage contentResponse = await Client.GetAsync($"/api/v1/email-templates/content?templateId={templateId}");
        GetEmailTemplateContentResponse contentResponseBody = await contentResponse.ParseContentAsync<GetEmailTemplateContentResponse>();
        Assert.That(contentResponseBody, Is.Not.Null);
        Assert.That(contentResponseBody.Url, Is.Not.Null);
    }

    [Test]
    public async Task ShouldReturnUnauthorizedWhenChangingContentsAsNonAdmin()
    {
        // Given: A non-admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(email, ThePassword);
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: Changing template contents
        using MultipartFormDataContent form = new();
        form.Add(new StringContent(Guid.NewGuid().ToString()), "templateID");
        form.Add(new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("[]"))), "templateFile", "template.json");

        HttpResponseMessage response = await Client.PutAsync("/api/v1/email-templates/content", form);

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

        // When: Changing contents with invalid ID
        using MultipartFormDataContent form = new();
        form.Add(new StringContent("not-a-guid"), "templateID");
        form.Add(new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("test"))), "templateFile", "t.txt");

        HttpResponseMessage response = await Client.PutAsync("/api/v1/email-templates/content", form);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        BadRequestResponse responseEntity = await response.ParseContentAsync<BadRequestResponse>();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(EmailTemplateErrors.InvalidTempalteID));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenFileIsMissing()
    {
        // Given: An admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: Changing contents without file
        using MultipartFormDataContent form = new();
        form.Add(new StringContent(Guid.NewGuid().ToString()), "templateID");

        HttpResponseMessage response = await Client.PutAsync("/api/v1/email-templates/content", form);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenTemplateNotFound()
    {
        // Given: An admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: Changing contents of a non-existent template
        using MultipartFormDataContent form = new();
        form.Add(new StringContent(Guid.NewGuid().ToString()), "templateID");
        form.Add(new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("content"))), "templateFile", "t.json");

        HttpResponseMessage response = await Client.PutAsync("/api/v1/email-templates/content", form);

        // Then: The handler should return TemplateNotFound
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        BadRequestResponse responseEntity = await response.ParseContentAsync<BadRequestResponse>();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(EmailTemplateErrors.TemplateNotFound));
    }
}
