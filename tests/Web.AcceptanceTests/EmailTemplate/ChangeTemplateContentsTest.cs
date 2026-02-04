using System.Net;
using System.Net.Http.Headers;
using NUnit.Framework;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.EmailTemplate;

[TestFixture]
public class ChangeTemplateContentsTest : GenericAcceptanceTest<VibraHekaProgram>
{
    [Test]
    public async Task ShouldChangeTemplateContentsWhenUserIsAdmin()
    {
        // Given: An admin user and an existing template skeleton
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        
        string initialName = $"ContentTest-{TheFaker.Random.AlphaNumeric(8)}";
        HttpResponseMessage createResponse = await Client.PutAsync($"/api/v1/email-templates/create-skeleton?templateName={initialName}", null);
        ResponseEntity createEntity = await createResponse.GetAsResponseEntityAndContentAs<string>();
        string templateId = createEntity.GetContentAs<string>()!;

        // And: A new content file
        string newContent = "<html><body>Updated Content</body></html>";
        using MultipartFormDataContent form = new MultipartFormDataContent();
        form.Add(new StringContent(templateId), "TemplateID");
        form.Add(new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(newContent))), "TemplateFile", "template.html");

        // When: Changing template contents
        HttpResponseMessage response = await Client.PatchAsync("/api/v1/email-templates/change-contents", form);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Happy Path check: Verify content actually updated
        HttpResponseMessage contentResponse = await Client.GetAsync($"/api/v1/email-templates/contents?templateID={templateId}");
        ResponseEntity contentEntity = await contentResponse.GetAsResponseEntityAndContentAs<string>();
        Assert.That(contentEntity.Content, Is.EqualTo(newContent));
    }

    [Test]
    public async Task ShouldReturnUnauthorizedWhenChangingContentsAsNonAdmin()
    {
        // Given: A non-admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: Changing template contents
        using MultipartFormDataContent form = new MultipartFormDataContent();
        form.Add(new StringContent(Guid.NewGuid().ToString()), "TemplateID");
        form.Add(new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("[]"))), "TemplateFile", "template.json");
        
        HttpResponseMessage response = await Client.PatchAsync("/api/v1/email-templates/change-contents", form);

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

        // When: Changing contents with invalid ID
        using MultipartFormDataContent form = new MultipartFormDataContent();
        form.Add(new StringContent("not-a-guid"), "TemplateID");
        form.Add(new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("test"))), "TemplateFile", "t.txt");
        
        HttpResponseMessage response = await Client.PatchAsync("/api/v1/email-templates/change-contents", form);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(EmailTemplateErrors.InvalidTempalteID));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenFileIsMissing()
    {
        // Given: An admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: Changing contents without file
        using MultipartFormDataContent form = new MultipartFormDataContent();
        form.Add(new StringContent(Guid.NewGuid().ToString()), "TemplateID");
        
        HttpResponseMessage response = await Client.PatchAsync("/api/v1/email-templates/change-contents", form);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
    
    [Test]
    public async Task ShouldReturnBadRequestWhenTemplateNotFound()
    {
        // Given: An admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: Changing contents of a non-existent template
        using MultipartFormDataContent form = new MultipartFormDataContent();
        form.Add(new StringContent(Guid.NewGuid().ToString()), "TemplateID");
        form.Add(new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("content"))), "TemplateFile", "t.json");

        HttpResponseMessage response = await Client.PatchAsync("/api/v1/email-templates/change-contents", form);

        // Then: The handler should return TemplateNotFound
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(EmailTemplateErrors.TemplateNotFound));
    }
}
