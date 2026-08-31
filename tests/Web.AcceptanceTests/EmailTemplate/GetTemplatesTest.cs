using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NUnit.Framework;
using VibraHeka.Web.EmailTemplates;
using VibraHeka.Web.AcceptanceTests.Generic;
using VibraHeka.Web.AcceptanceTests.Utils;
using VibraHeka.Web.Authentication;

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
        AuthenticateUserResponse authResult = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

        // When: requesting all templates.
        HttpResponseMessage response = await Client.GetAsync("/api/v1/email-templates");

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        GetTemplatesResponse responseEntity = await response.ParseContentAsync<GetTemplatesResponse>();
        IEnumerable<SimpleEmailTemplateDTO> templates = responseEntity.Templates;

        
        Assert.That(templates, Is.Not.Null);
        foreach (SimpleEmailTemplateDTO template in templates)
        {
            Assert.That(template.ID, Is.Not.EqualTo(Guid.Empty));
        }
    }

    [Test]
    public async Task ShouldReflectNewlyCreatedTemplateInList()
    {
        // Given: An admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // And: A newly created template
        string templateName = $"NewListTemplate-{TheFaker.Random.AlphaNumeric(8)}";
        CreateEmailTemplateRequest createRequest = new()
        {
            Name = templateName
        };
        await Client.PutAsJsonAsync("/api/v1/email-templates", createRequest);

        // When: requesting all templates.
        HttpResponseMessage response = await Client.GetAsync("/api/v1/email-templates");

        // Then: The list should contain the new template
        GetTemplatesResponse responseEntity = await response.ParseContentAsync<GetTemplatesResponse>();
        IEnumerable<SimpleEmailTemplateDTO> templates = responseEntity.Templates;

        Assert.That(templates, Is.Not.Null);
        Assert.That(templates.Any(t => t.Name == templateName), Is.True);
    }

    [Test]
    public async Task ShouldReturnUnauthorizedWhenUserIsNotAdmin()
    {
        // Given: a non-admin user to verify authorization is enforced.
        string email = TheFaker.Internet.Email();
        string username = TheFaker.Internet.UserName();

        await RegisterAndConfirmUser(username, email, ThePassword);
        AuthenticateUserResponse authResult = await AuthenticateUser(email, ThePassword);
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
        Client.DefaultRequestHeaders.Remove("Authorization");

        // When: requesting all templates.
        HttpResponseMessage response = await Client.GetAsync("api/v1/email-templates");

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
