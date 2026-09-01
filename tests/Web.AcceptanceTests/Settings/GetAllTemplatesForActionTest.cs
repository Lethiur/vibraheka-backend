using System.Net;
using System.Net.Http.Headers;
using NUnit.Framework;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Web.AcceptanceTests.Generic;
using VibraHeka.Web.AcceptanceTests.Utils;
using VibraHeka.Web.Authentication;
using VibraHeka.Web.Settings.Controllers;

namespace VibraHeka.Web.AcceptanceTests.Settings;

[TestFixture]
public class GetAllTemplatesForActionTest : GenericAcceptanceTest<VibraHekaProgram>
{
    [Test]
    public async Task ShouldReturnUnauthorizedWhenRequestIsUnauthenticated()
    {
        // Given: no authenticated user context.

        // When: requesting all template associations.
        HttpResponseMessage response = await Client.GetAsync("api/v1/settings/all-templates");

        // Then: endpoint should reject request with unauthorized.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldReturnOkAndTemplatesListWhenUserIsAdmin()
    {
        // Given: An authenticated admin
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Person.FullName, email, ThePassword);
        AuthenticateUserResponse authResult = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

        // When: Requesting all templates for actions
        HttpResponseMessage response = await Client.GetAsync("api/v1/settings/all-templates");

        // Then: Should return 200 OK
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        GetTemplateResponse responseEntity = await response.ParseContentAsync<GetTemplateResponse>();
        List<TemplateForActionDTO> templates = responseEntity.TemplateList;
        Assert.That(templates, Is.Not.Null);
        foreach (TemplateForActionDTO template in templates)
        {
            Assert.That(template.Id, Is.Not.EqualTo(Guid.Empty));
        }
    }

    [Test]
    public async Task ShouldReturnUnauthorizedWhenNonAdminAttemptsToGetAllTemplatesForActions()
    {
        // Given: A registered and confirmed standard user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(email, ThePassword);

        // And: The user is authenticated
        AuthenticateUserResponse authResult = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

        // When: A non-admin requests all templates for actions
        HttpResponseMessage response = await Client.GetAsync("api/v1/settings/all-templates");

        // Then: Should return 401 Unauthorized
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
