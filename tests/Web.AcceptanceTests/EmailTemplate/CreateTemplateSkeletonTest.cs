using System.Net;
using System.Net.Http.Headers;
using NUnit.Framework;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Web.AcceptanceTests.Generic;
using VibraHeka.Web.Entities;

namespace VibraHeka.Web.AcceptanceTests.EmailTemplate;

[TestFixture]
public class CreateTemplateSkeletonTest : GenericAcceptanceTest<VibraHekaProgram>
{
    [Test]
    public async Task ShouldCreateTemplateSkeletonWhenUserIsAdmin()
    {
        // Given: An admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        
        string templateName = $"Skeleton-{TheFaker.Random.AlphaNumeric(8)}";

        // When: Creating a skeleton
        HttpResponseMessage response = await Client.PutAsync($"/api/v1/email-templates/create-skeleton?templateName={templateName}", null);

        // Then: Should return 200 OK and the template ID
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        ResponseEntity responseEntity = await response.GetAsResponseEntityAndContentAs<string>();
        string? templateId = responseEntity.GetContentAs<string>();
        Assert.That(templateId, Is.Not.Null);

        // Verify the skeleton exists in the summary list (Happy Path check)
        HttpResponseMessage listResponse = await Client.GetAsync("/api/v1/email-templates");
        ResponseEntity listResponseEntity = await listResponse.GetAsResponseEntityAndContentAs<IEnumerable<EmailTemplateResponseDTO>>();
        IEnumerable<EmailTemplateResponseDTO>? templates = listResponseEntity.GetContentAs<IEnumerable<EmailTemplateResponseDTO>>();
        Assert.That(templates!.Any(t => t.TemplateID == templateId && t.TemplateName == templateName), Is.True);
    }

    [Test]
    public async Task ShouldReturnUnauthorizedWhenCreatingSkeletonAsNonAdmin()
    {
        // Given: A non-admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: Creating a skeleton
        HttpResponseMessage response = await Client.PutAsync("/api/v1/email-templates/create-skeleton?templateName=Unauthorized", null);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenTemplateNameIsTooShort()
    {
        // Given: An admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: Creating a skeleton with a name too short
        HttpResponseMessage response = await Client.PutAsync("/api/v1/email-templates/create-skeleton?templateName=Ab", null);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(Domain.Exceptions.EmailTemplateErrors.InvalidTemplateName));
    }
}
