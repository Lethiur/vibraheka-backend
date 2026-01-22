using System.Net;
using System.Net.Http.Headers;
using System.Text;
using NUnit.Framework;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.EmailTemplate;

[TestFixture]
public class CreateNewEmailTemplateTest : GenericAcceptanceTest<VibraHekaProgram>
{
    [Test]
    public async Task ShouldReturnUnauthorizedWhenCreateNewEmailTemplateIsCalledWithoutToken()
    {
        // Given
        using var form = CreateValidMultipartForm(
            templateName: $"template-{TheFaker.Random.AlphaNumeric(8)}",
            fileName: "template.json",
            fileContent: """{"template":"Hello","subject":"World"}""");

        // When
        using HttpResponseMessage response = await Client.PutAsync("/api/v1/email-templates/create", form);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldReturnOkWhenCreateNewEmailTemplateIsCalledWithValidRequestAndAuthorizedUser()
    {
        // Given
        string username = TheFaker.Internet.UserName();
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(username, email, ThePassword);

        var auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        using var form = CreateValidMultipartForm(
            templateName: $"template-{TheFaker.Random.AlphaNumeric(8)}",
            fileName: "template.json",
            fileContent: """{"template":"Hello","subject":"World"}""");

        // When
        using HttpResponseMessage response = await Client.PutAsync("/api/v1/email-templates/create", form);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenCreateNewEmailTemplateIsCalledWithoutFile()
    {
        // Given
        string username = TheFaker.Internet.UserName();
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(username, email, ThePassword);

        var auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent($"template-{TheFaker.Random.AlphaNumeric(8)}", Encoding.UTF8), "TemplateName");
        // Nota: no añadimos "File" para forzar request inválida

        // When
        using HttpResponseMessage response = await Client.PutAsync("/api/v1/email-templates/create", form);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    private static MultipartFormDataContent CreateValidMultipartForm(string templateName, string fileName,
        string fileContent)
    {
        var form = new MultipartFormDataContent();

        form.Add(new StringContent(templateName, Encoding.UTF8), "TemplateName");

        var bytes = Encoding.UTF8.GetBytes(fileContent);
        var fileStream = new MemoryStream(bytes);

        var filePart = new StreamContent(fileStream);
        filePart.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        form.Add(filePart, "File", fileName);

        return form;
    }
}
