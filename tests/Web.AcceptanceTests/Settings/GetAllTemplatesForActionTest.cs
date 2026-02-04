using System.Net;
using System.Net.Http.Headers;
using NUnit.Framework;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.Settings;

[TestFixture]
public class GetAllTemplatesForActionTest : GenericAcceptanceTest<VibraHekaProgram>
{
    [Test]
    public async Task ShouldReturnOkAndTemplatesListWhenUserIsAdmin()
    {
        // Given: An authenticated admin
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Person.FullName, email, ThePassword);
        AuthenticationResult authResult = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

        // When: Requesting all templates for actions
        HttpResponseMessage response = await Client.GetAsync("api/v1/settings/all-templates");

        // Then: Should return 200 OK
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        ResponseEntity responseEntity = await response.GetAsResponseEntityAndContentAs<IEnumerable<TemplateForActionEntity>>();
        Assert.That(responseEntity.Success, Is.True);
        Assert.That(responseEntity.Content, Is.Not.Null);
    }

    [Test]
    public async Task ShouldReturnUnauthorizedWhenNonAdminAttemptsToGetAllTemplatesForActions()
    {
        // Given: A registered and confirmed standard user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(TheFaker.Person.FullName, email, ThePassword);
        
        // And: The user is authenticated
        AuthenticationResult authResult = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

        // When: A non-admin requests all templates for actions
        HttpResponseMessage response = await Client.GetAsync("api/v1/settings/all-templates");

        // Then: Should return 401 Unauthorized
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
