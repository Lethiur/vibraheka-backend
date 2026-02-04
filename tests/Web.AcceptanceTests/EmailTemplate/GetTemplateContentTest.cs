using System.Net;
using System.Net.Http.Headers;
using NUnit.Framework;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Web.AcceptanceTests.Generic;
using static System.Text.Encoding;

namespace VibraHeka.Web.AcceptanceTests.EmailTemplate;

[TestFixture]
public class GetTemplateContentTest : GenericAcceptanceTest<VibraHekaProgram>
{
    [Test]
    public async Task ShouldReturnTemplateContentWhenTemplateHasFile()
    {
        // Given: An admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        
        // And: A template created WITH a file
        string templateName = $"FullTemplateContent-{TheFaker.Random.AlphaNumeric(8)}";
        string content = "<html>Sample Content</html>";
        
        
        using MultipartFormDataContent form = CreateValidMultipartForm(
            templateName: templateName,
            fileName: "template.json",
            fileContent: content);

        
        HttpResponseMessage httpResponseMessage = await Client.PutAsync("/api/v1/email-templates/create", form);
        Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));


        HttpResponseMessage listResponse = await Client.GetAsync("/api/v1/email-templates");
        ResponseEntity listEntity = await listResponse.GetAsResponseEntityAndContentAs<List<EmailEntity>>();
        EmailEntity template = listEntity.GetContentAs<List<EmailEntity>>()!
            .First(t => t.Name == templateName);

        // When: Getting the content
        HttpResponseMessage response = await Client.GetAsync($"/api/v1/email-templates/contents?templateID={template.ID}");

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        ResponseEntity contentEntity = await response.GetAsResponseEntityAndContentAs<string>();
        Assert.That(contentEntity.Content, Is.EqualTo(content));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenOnlySkeletonExists()
    {
        // Given: An admin user and a skeleton (no file)
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        
        string templateName = $"SkeletonOnly-{TheFaker.Random.AlphaNumeric(8)}";
        HttpResponseMessage createResponse = await Client.PutAsync($"/api/v1/email-templates/create-skeleton?templateName={templateName}", null);
        ResponseEntity createEntity = await createResponse.GetAsResponseEntityAndContentAs<string>();
        string templateId = createEntity.GetContentAs<string>()!;

        // When: Getting the content
        HttpResponseMessage response = await Client.GetAsync($"/api/v1/email-templates/contents?templateID={templateId}");

        // Then: Should return BadRequest with TemplateNotFound error because S3 file doesn't exist
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(EmailTemplateErrors.TemplateNotFound));
    }

    [Test]
    public async Task ShouldReturnUnauthorizedWhenGettingContentAsNonAdmin()
    {
        // Given: A non-admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: Getting template contents
        HttpResponseMessage response = await Client.GetAsync($"/api/v1/email-templates/contents?templateID={Guid.NewGuid()}");

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

        // When: Getting contents with invalid ID
        HttpResponseMessage response = await Client.GetAsync("/api/v1/email-templates/contents?templateID=not-a-guid");

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

        // When: Getting contents of a non-existent template
        HttpResponseMessage response = await Client.GetAsync($"/api/v1/email-templates/contents?templateID={Guid.NewGuid()}");

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(EmailTemplateErrors.TemplateNotFound));
    }
}
