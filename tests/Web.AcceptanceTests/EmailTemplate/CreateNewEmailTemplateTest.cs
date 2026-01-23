using System.Net;
using System.Net.Http.Headers;
using System.Text;
using NUnit.Framework;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.EmailTemplate;

[TestFixture]
public class CreateNewEmailTemplateTest : GenericAcceptanceTest<VibraHekaProgram>
{
    [Test]
    public async Task ShouldReturnUnauthorizedWhenCreateNewEmailTemplateIsCalledWithoutToken()
    {
        // Given: a valid request without authentication to verify unauthorized access.
        using MultipartFormDataContent form = CreateValidMultipartForm(
            templateName: $"template-{TheFaker.Random.AlphaNumeric(8)}",
            fileName: "template.json",
            fileContent: """{"template":"Hello","subject":"World"}""");

        // When: submitting the create-template request.
        using HttpResponseMessage response = await Client.PutAsync("/api/v1/email-templates/create", form);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldReturnOkWhenCreateNewEmailTemplateIsCalledWithValidRequestAndAuthorizedUser()
    {
        // Given: an admin user and a valid template request to verify success.
        string username = TheFaker.Internet.UserName();
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(username, email, ThePassword);

        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        using MultipartFormDataContent form = CreateValidMultipartForm(
            templateName: $"template-{TheFaker.Random.AlphaNumeric(8)}",
            fileName: "template.json",
            fileContent: """{"template":"Hello","subject":"World"}""");

        // When: submitting the create-template request.
        using HttpResponseMessage response = await Client.PutAsync("/api/v1/email-templates/create", form);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenCreateNewEmailTemplateIsCalledWithoutFile()
    {
        // Given: an admin request without a file to verify validation errors.
        string username = TheFaker.Internet.UserName();
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(username, email, ThePassword);

        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        using MultipartFormDataContent form = new MultipartFormDataContent();
        form.Add(new StringContent($"template-{TheFaker.Random.AlphaNumeric(8)}", Encoding.UTF8), "TemplateName");
        // Note: "File" is intentionally omitted to force an invalid request.

        // When: submitting the create-template request.
        using HttpResponseMessage response = await Client.PutAsync("/api/v1/email-templates/create", form);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ShouldReturnUnauthorizedWhenCreateNewEmailTemplateIsCalledByNonAdmin()
    {
        // Given: a non-admin user to verify authorization is enforced.
        string username = TheFaker.Internet.UserName();
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(username, email, ThePassword);

        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        using MultipartFormDataContent form = CreateValidMultipartForm(
            templateName: $"template-{TheFaker.Random.AlphaNumeric(8)}",
            fileName: "template.json",
            fileContent: """{"template":"Hello","subject":"World"}""");

        // When: submitting the create-template request.
        using HttpResponseMessage response = await Client.PutAsync("/api/v1/email-templates/create", form);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenTemplateNameIsInvalid()
    {
        // Given: an admin request with an invalid name to verify validation errors.
        string username = TheFaker.Internet.UserName();
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(username, email, ThePassword);

        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        using MultipartFormDataContent form = CreateValidMultipartForm(
            templateName: "AB",
            fileName: "template.json",
            fileContent: """{"template":"Hello","subject":"World"}""");

        // When: submitting the create-template request.
        using HttpResponseMessage response = await Client.PutAsync("/api/v1/email-templates/create", form);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.Success, Is.False);
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(EmailTemplateErrors.InvalidTemplateName));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenTemplateContentIsInvalidJson()
    {
        // Given: an admin request with invalid JSON to verify content validation.
        string username = TheFaker.Internet.UserName();
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(username, email, ThePassword);

        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        using MultipartFormDataContent form = CreateValidMultipartForm(
            templateName: $"template-{TheFaker.Random.AlphaNumeric(8)}",
            fileName: "template.json",
            fileContent: "not-json");

        // When: submitting the create-template request.
        using HttpResponseMessage response = await Client.PutAsync("/api/v1/email-templates/create", form);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.Success, Is.False);
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(EmailTemplateErrors.InvalidTemplateContent));
    }

    private static MultipartFormDataContent CreateValidMultipartForm(string templateName, string fileName,
        string fileContent)
    {
        MultipartFormDataContent form = new MultipartFormDataContent();

        form.Add(new StringContent(templateName, Encoding.UTF8), "TemplateName");

        byte[] bytes = Encoding.UTF8.GetBytes(fileContent);
        MemoryStream fileStream = new MemoryStream(bytes);

        StreamContent filePart = new StreamContent(fileStream);
        filePart.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        form.Add(filePart, "File", fileName);

        return form;
    }
}
