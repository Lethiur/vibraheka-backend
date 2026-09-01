using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NUnit.Framework;
using VibraHeka.Web.EmailTemplates;
using VibraHeka.Web.AcceptanceTests.Generic;
using VibraHeka.Web.AcceptanceTests.Utils;
using VibraHeka.Web.Authentication;
using static VibraHeka.Domain.Exceptions.EmailTemplateErrors;
using BadRequestResponse = VibraHeka.Web.EmailTemplates.BadRequestResponse;

namespace VibraHeka.Web.AcceptanceTests.EmailTemplate;

[TestFixture]
public class CreateTemplateSkeletonTest : GenericAcceptanceTest<VibraHekaProgram>
{
    [Test]
    public async Task ShouldReturnUnauthorizedWhenCreatingSkeletonWithoutAuthentication()
    {
        // Given: no bearer token in request headers.
        string templateName = $"Skeleton-{TheFaker.Random.AlphaNumeric(8)}";

        // When: creating template skeleton.
        CreateEmailTemplateRequest createRequest = new()
        {
            Name = templateName
        };
        HttpResponseMessage response = await Client.PutAsJsonAsync("/api/v1/email-templates", createRequest);

        // Then: endpoint should reject unauthenticated access.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldCreateTemplateSkeletonWhenUserIsAdmin()
    {
        // Given: An admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        string templateName = $"Skeleton-{TheFaker.Random.AlphaNumeric(8)}";

        // When: Creating a skeleton
        CreateEmailTemplateRequest createRequest = new()
        {
            Name = templateName
        };
        HttpResponseMessage response = await Client.PutAsJsonAsync("/api/v1/email-templates", createRequest);

        // Then: Should return 200 OK and the template ID
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        CreateEmailTemplateResponse responseEntity = await response.ParseContentAsync<CreateEmailTemplateResponse>();
        Assert.That(responseEntity, Is.Not.Null);

        // Verify the skeleton exists in the summary list (Happy Path check)
        HttpResponseMessage listResponse = await Client.GetAsync("/api/v1/email-templates");
        GetTemplatesResponse listResponseEntity = await listResponse.ParseContentAsync<GetTemplatesResponse>();
        IEnumerable<SimpleEmailTemplateDTO> templates = listResponseEntity.Templates;
        Assert.That(templates.Any(t => t.ID == responseEntity.TemplateId && t.Name == templateName), Is.True);
    }

    [Test]
    public async Task ShouldReturnUnauthorizedWhenCreatingSkeletonAsNonAdmin()
    {
        // Given: A non-admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(email, ThePassword);
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: Creating a skeleton
        HttpResponseMessage response = await Client.PutAsJsonAsync("/api/v1/email-templates", new CreateEmailTemplateRequest
        {
            Name = "Unauthorized"
        });

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenTemplateNameIsTooShort()
    {
        // Given: An admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: Creating a skeleton with a name too short
        HttpResponseMessage response = await Client.PutAsJsonAsync("/api/v1/email-templates", new CreateEmailTemplateRequest
        {
            Name = "Ab"
        });

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        BadRequestResponse responseEntity = await response.ParseContentAsync<BadRequestResponse>();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(InvalidTemplateName));
    }
}
