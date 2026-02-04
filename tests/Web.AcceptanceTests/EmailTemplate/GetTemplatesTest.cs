using System.Net;
using System.Net.Http.Headers;
using System.Text;
using NUnit.Framework;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Web.AcceptanceTests.Generic;
using VibraHeka.Web.Entities;

namespace VibraHeka.Web.AcceptanceTests.EmailTemplate;

[TestFixture]
public class GetTemplatesTest : GenericAcceptanceTest<VibraHekaProgram>
{
    [Test]
    public async Task ShouldReturnOkAndTemplatesWhenUserIsAdmin()
    {
        // Given: an admin user to verify access and successful response.
        string email = TheFaker.Internet.Email();
        string username = TheFaker.Internet.UserName();

        await RegisterAndConfirmAdmin(username, email, ThePassword);
        AuthenticationResult authResult = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

        // When: requesting all templates.
        HttpResponseMessage response = await Client.GetAsync("api/v1/email-templates");

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        ResponseEntity responseEntity = await response.GetAsResponseEntityAndContentAs<IEnumerable<EmailEntity>>();
        IEnumerable<EmailEntity>? templates = responseEntity.GetContentAs<IEnumerable<EmailEntity>>();

        Assert.That(responseEntity.Success, Is.True);
        Assert.That(templates, Is.Not.Null);
    }

    [Test]
    public async Task ShouldReflectNewlyCreatedTemplateInList()
    {
        // Given: An admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        
        // And: A newly created template
        string templateName = $"NewListTemplate-{TheFaker.Random.AlphaNumeric(8)}";
        using MultipartFormDataContent form = new MultipartFormDataContent();
        form.Add(new StringContent(templateName), "TemplateName");
        form.Add(new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("{}"))), "File", "t.json");
        await Client.PutAsync("/api/v1/email-templates/create", form);

        // When: requesting all templates.
        HttpResponseMessage response = await Client.GetAsync("api/v1/email-templates");

        // Then: The list should contain the new template
        ResponseEntity responseEntity = await response.GetAsResponseEntityAndContentAs<IEnumerable<EmailTemplateResponseDTO>>();
        IEnumerable<EmailTemplateResponseDTO>? templates = responseEntity.GetContentAs<IEnumerable<EmailTemplateResponseDTO>>();
        
        Assert.That(templates, Is.Not.Null);
        Assert.That(templates!.Any(t => t.TemplateName == templateName), Is.True);
    }

    [Test]
    public async Task ShouldReturnUnauthorizedWhenUserIsNotAdmin()
    {
        // Given: a non-admin user to verify authorization is enforced.
        string email = TheFaker.Internet.Email();
        string username = TheFaker.Internet.UserName();

        await RegisterAndConfirmUser(username, email, ThePassword);
        AuthenticationResult authResult = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

        // When: requesting all templates.
        HttpResponseMessage response = await Client.GetAsync("api/v1/email-templates");

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldReturnUnauthorizedWhenRequestIsUnauthenticated()
    {
        // Given: no authentication header to verify unauthenticated access is rejected.

        // When: requesting all templates.
        HttpResponseMessage response = await Client.GetAsync("api/v1/email-templates");

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
