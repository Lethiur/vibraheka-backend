using System.Net;
using System.Net.Http.Headers;
using NUnit.Framework;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Web.AcceptanceTests.Generic;

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
