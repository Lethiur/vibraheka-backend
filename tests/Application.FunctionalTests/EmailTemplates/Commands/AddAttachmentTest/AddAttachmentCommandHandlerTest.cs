using System.Text;
using CSharpFunctionalExtensions;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.EmailTemplates.Commands.AddAttachment;
using VibraHeka.Domain.EmailTemplates.Ports.Out;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Domain.User.Services;
using static CSharpFunctionalExtensions.Result;

namespace VibraHeka.Application.FunctionalTests.EmailTemplates.Commands.AddAttachmentTest;

public class AddAttachmentCommandHandlerTest
{
    private const string DefaultUserId = "user-123";
    private const string DefaultTemplateId = "template-123";
    private const string DefaultAttachmentName = "file.pdf";
    private const string DefaultAttachmentUrl = "https://cdn.example.com/attachments/file.pdf";
    private static readonly CancellationToken NoCancellation = CancellationToken.None;

    private Mock<ICurrentUserService> _currentUserServiceMock;
    
    private Mock<EmailTemplatePort> _emailPort;
    private Mock<EmailTemplateContentPort> _emailContentPort;
    private AddAttachmentCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _emailPort = new Mock<EmailTemplatePort>();
        _emailContentPort = new Mock<EmailTemplateContentPort>();

        _handler = new AddAttachmentCommandHandler(
            _emailPort.Object,
            _emailContentPort.Object);
    }

    private static MemoryStream CreateStream(string content = "file-content")
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(content));
    }

    private static AddAttachmentCommand CreateCommand(Stream stream, string templateId = DefaultTemplateId,
        string attachmentName = DefaultAttachmentName)
    {
        return new AddAttachmentCommand(stream, templateId, attachmentName);
    }

    private static EmailTemplateEntity CreateTemplateEntity(string templateId = DefaultTemplateId)
    {
        return new EmailTemplateEntity { TemplateID = templateId };
    }

    [Test]
    [Description(
        "Given a valid admin user and a template, when adding an attachment, then it should store the URL and save the template")]
    public async Task ShouldAddAttachmentAndSaveTemplateWhenRequestIsValid()
    {
        // Given: an admin user and a valid template to verify the attachment is saved and persisted.
        const string attachmentName = "invoice.pdf";
        const string attachmentUrl = "https://cdn.example.com/attachments/invoice.pdf";
        MemoryStream fileStream = CreateStream();
        EmailTemplateEntity templateTemplateEntity = CreateTemplateEntity();
        AddAttachmentCommand command = CreateCommand(fileStream, DefaultTemplateId, attachmentName);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(DefaultUserId);
        _emailPort.Setup(x => x.GetTemplateByID(DefaultTemplateId, CancellationToken.None))
            .ReturnsAsync(Success(templateTemplateEntity));
        _emailContentPort.Setup(x => x.SaveAttachment(templateTemplateEntity.TemplateID, fileStream, attachmentName, NoCancellation))
            .ReturnsAsync(Success(attachmentUrl));
        _emailPort.Setup(x => x.SaveEmailTemplate(templateTemplateEntity, NoCancellation))
            .ReturnsAsync(Success("template-saved"));

        // When: handling the add-attachment command.
        Result<string> result = await _handler.Handle(command, NoCancellation);

        // Then
        Assert.That(result.IsSuccess);
        _emailContentPort.Verify(
            x => x.SaveAttachment(templateTemplateEntity.TemplateID, fileStream, attachmentName, NoCancellation),
            Times.Once);
        _emailPort.Verify(
            x => x.SaveEmailTemplate(
                It.Is<EmailTemplateEntity>(e => e.Attachments.Contains(attachmentUrl)),
                NoCancellation),
            Times.Once);
    }
    

    [Test]
    [Description(
        "Given a valid admin user but template is null, when adding an attachment, then it should return TemplateNotFound error")]
    public async Task ShouldReturnTemplateNotFoundWhenTemplateIsNull()
    {
        // Given: a valid admin user but missing template to verify template not found error.
        AddAttachmentCommand command = CreateCommand(new MemoryStream(), DefaultTemplateId, DefaultAttachmentName);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(DefaultUserId);
        _emailPort.Setup(x => x.GetTemplateByID(DefaultTemplateId, CancellationToken.None))
            .ReturnsAsync(Failure<EmailTemplateEntity>(EmailTemplateErrors.TemplateNotFound));

        // When: handling the add-attachment command.
        Result<string> result = await _handler.Handle(command, NoCancellation);

        // Then
        Assert.That(result.IsFailure);
        Assert.That(result.Error, Is.EqualTo(EmailTemplateErrors.TemplateNotFound));
        _emailContentPort.Verify(
            x => x.SaveAttachment(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    [Description(
        "Given a valid admin user but storage fails, when adding an attachment, then it should return storage error")]
    public async Task ShouldReturnStorageErrorWhenAddAttachmentFails()
    {
        // Given: a valid admin request where storage fails to verify error propagation.
        const string errorMessage = "ST-001";
        MemoryStream fileStream = CreateStream();
        EmailTemplateEntity templateTemplateEntity = CreateTemplateEntity();
        AddAttachmentCommand command = CreateCommand(fileStream);

        _emailPort.Setup(x => x.GetTemplateByID(DefaultTemplateId, CancellationToken.None))
            .ReturnsAsync(Success(templateTemplateEntity));
        _emailContentPort.Setup(x => x.SaveAttachment(templateTemplateEntity.TemplateID, fileStream, DefaultAttachmentName, NoCancellation))
            .ReturnsAsync(Failure<string>(errorMessage));

        // When: handling the add-attachment command.
        Result<string> result = await _handler.Handle(command, NoCancellation);

        // Then
        Assert.That(result.IsFailure);
        Assert.That(result.Error, Is.EqualTo(errorMessage));
        _emailPort.Verify(
            x => x.SaveEmailTemplate(It.IsAny<EmailTemplateEntity>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    [Description(
        "Given a valid admin user but template save fails, when adding an attachment, then it should return template error")]
    public async Task ShouldReturnTemplateErrorWhenSaveEmailTemplateFails()
    {
        // Given: a valid admin request where saving the template fails to verify error propagation.
        const string errorMessage = "TPL-500";
        MemoryStream fileStream = CreateStream();
        EmailTemplateEntity templateTemplateEntity = CreateTemplateEntity();
        AddAttachmentCommand command = CreateCommand(fileStream);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(DefaultUserId);
        _emailPort.Setup(x => x.GetTemplateByID(DefaultTemplateId, CancellationToken.None))
            .ReturnsAsync(Success(templateTemplateEntity));
        _emailContentPort.Setup(x => x.SaveAttachment(templateTemplateEntity.TemplateID, fileStream, DefaultAttachmentName, NoCancellation))
            .ReturnsAsync(Success(DefaultAttachmentUrl));
        _emailPort.Setup(x => x.SaveEmailTemplate(templateTemplateEntity, NoCancellation))
            .ReturnsAsync(Failure<string>(errorMessage));

        // When: handling the add-attachment command.
        Result<string> result = await _handler.Handle(command, NoCancellation);

        // Then
        Assert.That(result.IsFailure);
        Assert.That(result.Error, Is.EqualTo(errorMessage));
    }
}
