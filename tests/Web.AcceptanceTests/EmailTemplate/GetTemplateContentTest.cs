using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NUnit.Framework;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Web.EmailTemplates;
using VibraHeka.Web.AcceptanceTests.Generic;
using VibraHeka.Web.AcceptanceTests.Utils;
using VibraHeka.Web.Authentication;
using BadRequestResponse = VibraHeka.Web.EmailTemplates.BadRequestResponse;

namespace VibraHeka.Web.AcceptanceTests.EmailTemplate;

[TestFixture]
public class GetTemplateContentTest : GenericAcceptanceTest<VibraHekaProgram>
{
    [Test]
    public async Task ShouldReturnUnauthorizedWhenGettingContentWithoutAuthentication()
    {
        // Given: request without an authorization header.
        // When: reading template contents endpoint.
        HttpResponseMessage response =
            await Client.GetAsync($"/api/v1/email-templates/content?templateId={Guid.NewGuid()}");

        // Then: endpoint should reject unauthenticated request.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldReturnTemplateContentWhenTemplateHasFile()
    {
        // Given: An admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // And: A template created WITH a file
        string templateName = $"FullTemplateContent-{TheFaker.Random.AlphaNumeric(8)}";
        CreateEmailTemplateRequest createRequest = new()
        {
            Name = templateName
        };

        HttpResponseMessage httpResponseMessage = await Client.PutAsJsonAsync("/api/v1/email-templates", createRequest);
        Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));


        HttpResponseMessage listResponse = await Client.GetAsync("/api/v1/email-templates");
        GetTemplatesResponse listEntity = await listResponse.ParseContentAsync<GetTemplatesResponse>();
        SimpleEmailTemplateDTO template = listEntity.Templates
            .First(t => t.Name == templateName);

        // When: Getting the content
        HttpResponseMessage response = await Client.GetAsync($"/api/v1/email-templates/content?templateId={template.ID}");

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        GetEmailTemplateContentResponse contentEntity = await response.ParseContentAsync<GetEmailTemplateContentResponse>();
        Assert.That(contentEntity, Is.Not.Null);
        Assert.That(contentEntity.Url, Is.Not.Null);
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenOnlySkeletonExists()
    {
        // Given: An admin user and a skeleton (no file)
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        string templateName = $"SkeletonOnly-{TheFaker.Random.AlphaNumeric(8)}";
        CreateEmailTemplateRequest createRequest = new()
        {
            Name = templateName
        };
        HttpResponseMessage createResponse = await Client.PutAsJsonAsync("/api/v1/email-templates", createRequest);
        
        Guid templateId = (await createResponse.ParseContentAsync<CreateEmailTemplateResponse>()).TemplateId;

        // When: Getting the content
        HttpResponseMessage response = await Client.GetAsync($"/api/v1/email-templates/content?templateId={templateId}");

        // Then: Should return BadRequest with TemplateNotFound error because S3 file doesn't exist
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        BadRequestResponse responseEntity = await response.ParseContentAsync<BadRequestResponse>();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(EmailTemplateErrors.TemplateNotFound));
    }

    [Test]
    public async Task ShouldReturnUnauthorizedWhenGettingContentAsNonAdmin()
    {
        // Given: A non-admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(email, ThePassword);
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: Getting template contents
        HttpResponseMessage response = await Client.GetAsync($"/api/v1/email-templates/content?templateId={Guid.NewGuid()}");

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenTemplateIdIsInvalid()
    {
        // Given: An admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: Getting contents with invalid ID
        HttpResponseMessage response = await Client.GetAsync("/api/v1/email-templates/content?templateId=not-a-guid");

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        BadRequestResponse responseEntity = await response.ParseContentAsync<BadRequestResponse>();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(EmailTemplateErrors.InvalidTempalteID));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenTemplateNotFound()
    {
        // Given: An admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Internet.UserName(), email, ThePassword);
        AuthenticateUserResponse auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // When: Getting contents of a non-existent template
        HttpResponseMessage response = await Client.GetAsync($"/api/v1/email-templates/content?templateId={Guid.NewGuid()}");

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        BadRequestResponse responseEntity = await response.ParseContentAsync<BadRequestResponse>();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(EmailTemplateErrors.TemplateNotFound));
    }
}
