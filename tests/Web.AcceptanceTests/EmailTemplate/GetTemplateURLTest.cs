using System.Net;
using System.Net.Http.Headers;
using NUnit.Framework;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.EmailTemplate;

[TestFixture]
public class GetTemplateURLTest : GenericAcceptanceTest<VibraHekaProgram>
{
    [Test]
    public async Task ShouldReturnTemplateURLWhenTemplateHasFile()
    {
        // Given: An admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        
        // And: A template created WITH a file (not a skeleton)
        string templateName = $"FullTemplate-{TheFaker.Random.AlphaNumeric(8)}";
        using MultipartFormDataContent form = new MultipartFormDataContent();
        form.Add(new StringContent(templateName), "TemplateName");
        form.Add(new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("<html>Sample</html>"))), "File", "sample.html");
        
        await Client.PutAsync("/api/v1/email-templates/create", form);
        
        // We need to get the ID. We can get all templates to find it.
        HttpResponseMessage listResponse = await Client.GetAsync("/api/v1/email-templates");
        ResponseEntity listEntity = await listResponse.GetAsResponseEntityAndContentAs<IEnumerable<EmailEntity>>();
        EmailEntity template = listEntity.GetContentAs<IEnumerable<EmailEntity>>()!
            .First(t => t.Name == templateName);

        // When: Getting the URL
        HttpResponseMessage response = await Client.GetAsync($"/api/v1/email-templates/url?TemplateID={template.ID}");

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        ResponseEntity urlEntity = await response.GetAsResponseEntityAndContentAs<string>();
        Assert.That(urlEntity.Content, Is.Not.Empty);
        Assert.That(urlEntity.Content!.ToString()!.StartsWith("http"), Is.True);
    }

    [Test]
    public async Task ShouldReturnUnauthorizedWhenGettingURLAsNonAdmin()
    {
        // Given: A non-admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: Getting a template URL
        HttpResponseMessage response = await Client.GetAsync($"/api/v1/email-templates/url?TemplateID={Guid.NewGuid()}");

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

        // When: Getting URL with invalid ID
        HttpResponseMessage response = await Client.GetAsync("/api/v1/email-templates/url?TemplateID=not-a-guid");

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

        // When: Getting URL of a non-existent template
        HttpResponseMessage response = await Client.GetAsync($"/api/v1/email-templates/url?TemplateID={Guid.NewGuid()}");

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(EmailTemplateErrors.TemplateNotFound));
    }
}
