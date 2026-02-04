using System.Net;
using System.Net.Http.Headers;
using System.Text;
using NUnit.Framework;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.EmailTemplate;

[TestFixture]
public class AddAttachmentToEmailTemplateTest : GenericAcceptanceTest<VibraHekaProgram>
{
    [Test]
    public async Task ShouldReturnUnauthorizedWhenAddAttachmentIsCalledWithoutToken()
    {
        // Given: a valid attachment request without authentication to verify unauthorized access.
        using MultipartFormDataContent form = CreateAttachmentForm(
            templateId: Guid.NewGuid().ToString(),
            attachmentName: "test.png",
            fileName: "test.png",
            contentType: "image/png",
            fileBytes: BuildPngBytes());

        // When: submitting the add-attachment request.
        using HttpResponseMessage response = await Client.PutAsync("/api/v1/email-templates/add-attachment", form);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldReturnUnauthorizedWhenUserIsNotAdmin()
    {
        // Given: a non-admin user to verify authorization enforcement.
        string email = TheFaker.Internet.Email();
        string username = TheFaker.Internet.UserName();
        string templateId = Guid.NewGuid().ToString("N");

        await InsertTemplateInDatabase(templateId);
        await RegisterAndConfirmUser(username, email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        using MultipartFormDataContent form = CreateAttachmentForm(
            templateId: templateId,
            attachmentName: "test.png",
            fileName: "test.png",
            contentType: "image/png",
            fileBytes: BuildPngBytes());

        // When: submitting the add-attachment request.
        using HttpResponseMessage response = await Client.PutAsync("/api/v1/email-templates/add-attachment", form);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenTemplateDoesNotExist()
    {
        // Given: an admin user with a missing template id to verify template-not-found handling.
        string email = TheFaker.Internet.Email();
        string username = TheFaker.Internet.UserName();
        string templateId = Guid.NewGuid().ToString("N");

        await RegisterAndConfirmAdmin(username, email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        using MultipartFormDataContent form = CreateAttachmentForm(
            templateId: templateId,
            attachmentName: "test.png",
            fileName: "test.png",
            contentType: "image/png",
            fileBytes: BuildPngBytes());

        // When: submitting the add-attachment request.
        using HttpResponseMessage response = await Client.PutAsync("/api/v1/email-templates/add-attachment", form);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.Success, Is.False);
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(EmailTemplateErrors.TemplateNotFound));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenAttachmentTypeIsInvalid()
    {
        // Given: a valid admin user and invalid file bytes to verify file-type validation.
        string email = TheFaker.Internet.Email();
        string username = TheFaker.Internet.UserName();
        string templateId = Guid.NewGuid().ToString("N");

        await InsertTemplateInDatabase(templateId);
        await RegisterAndConfirmAdmin(username, email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        byte[] invalidBytes = Encoding.UTF8.GetBytes("not-an-image-or-video");
        using MultipartFormDataContent form = CreateAttachmentForm(
            templateId: templateId,
            attachmentName: "test.txt",
            fileName: "test.txt",
            contentType: "text/plain",
            fileBytes: invalidBytes);

        // When: submitting the add-attachment request.
        using HttpResponseMessage response = await Client.PutAsync("/api/v1/email-templates/add-attachment", form);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.Success, Is.False);
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(EmailTemplateErrors.InvalidAttachmentContent));
    }

    [Test]
    public async Task ShouldReturnOkWhenAttachmentIsValidImage()
    {
        // Given: a valid admin user and an image file to verify successful upload.
        string email = TheFaker.Internet.Email();
        string username = TheFaker.Internet.UserName();
        string templateId = Guid.NewGuid().ToString("N");

        await InsertTemplateInDatabase(templateId);
        await RegisterAndConfirmAdmin(username, email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        using MultipartFormDataContent form = CreateAttachmentForm(
            templateId: templateId,
            attachmentName: "test.png",
            fileName: "test.png",
            contentType: "image/png",
            fileBytes: BuildPngBytes());

        // When: submitting the add-attachment request.
        using HttpResponseMessage response = await Client.PutAsync("/api/v1/email-templates/add-attachment", form);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.Success, Is.True);
        Assert.That(responseEntity.Content, Is.Not.Null);
    }

    [Test]
    public async Task ShouldReturnOkWhenAttachmentIsValidVideo()
    {
        // Given: a valid admin user and a video file to verify successful upload.
        string email = TheFaker.Internet.Email();
        string username = TheFaker.Internet.UserName();
        string templateId = Guid.NewGuid().ToString("N");

        await InsertTemplateInDatabase(templateId);
        await RegisterAndConfirmAdmin(username, email, ThePassword);
        AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        using MultipartFormDataContent form = CreateAttachmentForm(
            templateId: templateId,
            attachmentName: "test.mp4",
            fileName: "test.mp4",
            contentType: "video/mp4",
            fileBytes: BuildMp4Bytes());

        // When: submitting the add-attachment request.
        using HttpResponseMessage response = await Client.PutAsync("/api/v1/email-templates/add-attachment", form);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.Success, Is.True);
        Assert.That(responseEntity.Content, Is.Not.Null);
    }

    private async Task InsertTemplateInDatabase(string templateId)
    {
        IEmailTemplatesRepository repository = GetObjectFromFactory<IEmailTemplatesRepository>();

        EmailEntity template = new EmailEntity
        {
            ID = templateId,
            Name = "Acceptance Template",
            Path = "test",
            Created = DateTime.UtcNow,
            CreatedBy = "SystemTest"
        };

        await repository.SaveTemplate(template, CancellationToken.None);
    }

    private static MultipartFormDataContent CreateAttachmentForm(string templateId, string attachmentName,
        string fileName, string contentType, byte[] fileBytes)
    {
        MultipartFormDataContent form = new MultipartFormDataContent();

        form.Add(new StringContent(templateId), "TemplateID");
        form.Add(new StringContent(attachmentName), "AttachmentName");

        MemoryStream fileStream = new MemoryStream(fileBytes);
        StreamContent filePart = new StreamContent(fileStream);
        filePart.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        form.Add(filePart, "File", fileName);
        return form;
    }

    private static byte[] BuildPngBytes()
    {
        return new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00 };
    }

    private static byte[] BuildMp4Bytes()
    {
        return new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70, 0x6D, 0x70, 0x34, 0x32 };
    }
}
