using System.Text;
using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.EmailTemplates.Commands.CreateEmail;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Application.FunctionalTests.EmailTemplates.Commands.CreateEmailTemplateTest;

public class CreateEmailTemplateCommandHandlerTests
{
    private Mock<IEmailTemplateStorageService> _storageServiceMock;
    private Mock<IEmailTemplatesService> _templateServiceMock;
    private CreateEmailTemplateCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _storageServiceMock = new Mock<IEmailTemplateStorageService>();
        _templateServiceMock = new Mock<IEmailTemplatesService>();

        _handler = new CreateEmailTemplateCommandHandler(
            _templateServiceMock.Object,
            _storageServiceMock.Object
        );
    }

    [Test]
    [Description(
        "Given a valid admin user with a valid email template file, when creating an email template, then the template should be saved successfully")]
    public async Task ShouldCreateEmailTemplateWhenUserIsAdminAndRequestIsValid()
    {
        // Given
        string templateName = "Welcome Email";
        MemoryStream fileStream = new MemoryStream(Encoding.UTF8.GetBytes("{a: 3}"));
        CancellationToken cancellationToken = CancellationToken.None;
        CreateEmailTemplateCommand command = new CreateEmailTemplateCommand(fileStream, templateName);
            
        
        _storageServiceMock.Setup(x => x.SaveTemplate(It.IsAny<string>(), fileStream, cancellationToken))
            .ReturnsAsync(Result.Success("template-id"));
        _templateServiceMock.Setup(x => x.SaveEmailTemplate(It.IsAny<EmailEntity>(), cancellationToken))
            .ReturnsAsync("a");

        // When
        Result<Unit> result = await _handler.Handle(command, cancellationToken);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        _storageServiceMock.Verify(x => x.SaveTemplate(It.IsAny<string>(), fileStream, cancellationToken), Times.Once);
        _templateServiceMock.Verify(x => x.SaveEmailTemplate(It.IsAny<EmailEntity>(), cancellationToken), Times.Once);
    }
    
    
    [Test]
    [Description(
        "Given a valid admin user but storage service fails, when creating an email template, then it should return storage error")]
    public async Task ShouldReturnStorageErrorWhenStorageServiceFails()
    {
        // Given
      
        const string templateName = "Welcome Email";
        MemoryStream fileStream = new MemoryStream(Encoding.UTF8.GetBytes("Template content"));
        CancellationToken cancellationToken = CancellationToken.None;
        CreateEmailTemplateCommand command = new CreateEmailTemplateCommand(fileStream, templateName);

      
        _storageServiceMock.Setup(x => x.SaveTemplate(It.IsAny<string>(), fileStream, cancellationToken))
            .ReturnsAsync(Result.Failure<string>(InfrastructureFileManagementErrors.InvalidHash));

        // When
        Result<Unit> result = await _handler.Handle(command, cancellationToken);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(InfrastructureFileManagementErrors.InvalidHash));
        _templateServiceMock.Verify(
            x => x.SaveEmailTemplate(It.IsAny<EmailEntity>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    [Description(
        "Given a valid admin user with successfully saved file but template service fails, when creating an email template, then it should return template error")]
    public async Task ShouldReturnTemplateErrorWhenTemplateServiceFails()
    {
        // Given
        const string templateName = "Welcome Email";
        MemoryStream fileStream = new MemoryStream(Encoding.UTF8.GetBytes("Template content"));
        CancellationToken cancellationToken = CancellationToken.None;
        CreateEmailTemplateCommand command = new CreateEmailTemplateCommand(fileStream, templateName);

        _storageServiceMock.Setup(x => x.SaveTemplate(It.IsAny<string>(), fileStream, cancellationToken))
            .ReturnsAsync(Result.Success("template-id"));
        _templateServiceMock.Setup(x => x.SaveEmailTemplate(It.IsAny<EmailEntity>(), cancellationToken))
            .ReturnsAsync(Result.Failure<string>(InfrastructureFileManagementErrors.InvalidHash));

        // When
        Result<Unit> result = await _handler.Handle(command, cancellationToken);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(InfrastructureFileManagementErrors.InvalidHash));
    }

    [Test]
    [Description(
        "Given a valid admin user creating an email template, when the handler processes the request, then the email entity should contain correct name and matching ID and Path")]
    public async Task ShouldPassCorrectEmailEntityWhenSavingTemplate()
    {
        // Given
        const string templateName = "Welcome Email";
        MemoryStream fileStream = new MemoryStream(Encoding.UTF8.GetBytes("Template content"));
        CancellationToken cancellationToken = CancellationToken.None;
        CreateEmailTemplateCommand command = new CreateEmailTemplateCommand(fileStream, templateName);

        _storageServiceMock.Setup(x => x.SaveTemplate(It.IsAny<string>(), fileStream, cancellationToken))
            .ReturnsAsync(Result.Success("template-id"));
        _templateServiceMock.Setup(x => x.SaveEmailTemplate(It.IsAny<EmailEntity>(), cancellationToken))
            .ReturnsAsync("string");

        // When
        await _handler.Handle(command, cancellationToken);

        // Then
        _templateServiceMock.Verify(
            x => x.SaveEmailTemplate(
                It.Is<EmailEntity>(e =>
                    e.Name == templateName &&
                    e.Path == ("template-id") &&
                    !string.IsNullOrEmpty(e.ID)),
                cancellationToken),
            Times.Once);
    }

    [Test]
    [Description(
        "Given a valid admin user creating an email template, when the storage service is called, then it should be called exactly once with the correct parameters")]
    public async Task ShouldCallStorageServiceOnceWhenCreatingEmailTemplate()
    {
        // Given
        const string templateName = "Welcome Email";
        MemoryStream fileStream = new MemoryStream(Encoding.UTF8.GetBytes("Template content"));
        CancellationToken cancellationToken = CancellationToken.None;
        CreateEmailTemplateCommand command = new CreateEmailTemplateCommand(fileStream, templateName);

        _storageServiceMock.Setup(x => x.SaveTemplate(It.IsAny<string>(), fileStream, cancellationToken))
            .ReturnsAsync(Result.Success("template-id"));
        _templateServiceMock.Setup(x => x.SaveEmailTemplate(It.IsAny<EmailEntity>(), cancellationToken))
            .ReturnsAsync("string");

        // When
        await _handler.Handle(command, cancellationToken);

        // Then
        _storageServiceMock.Verify(
            x => x.SaveTemplate(It.IsAny<string>(), fileStream, cancellationToken),
            Times.Once);
    }

    [Test]
    [Description(
        "Given a valid admin user with a successfully created email template, when the operation completes, then the result should contain Unit.Value")]
    public async Task ShouldReturnUnitValueWhenEmailTemplateCreatedSuccessfully()
    {
        // Given
        const string templateName = "Welcome Email";
        MemoryStream fileStream = new MemoryStream(Encoding.UTF8.GetBytes("Template content"));
        CancellationToken cancellationToken = CancellationToken.None;
        CreateEmailTemplateCommand command = new CreateEmailTemplateCommand(fileStream, templateName);

        _storageServiceMock.Setup(x => x.SaveTemplate(It.IsAny<string>(), fileStream, cancellationToken))
            .ReturnsAsync(Result.Success("template-id"));
        _templateServiceMock.Setup(x => x.SaveEmailTemplate(It.IsAny<EmailEntity>(), cancellationToken))
            .ReturnsAsync("str");

        // When
        Result<Unit> result = await _handler.Handle(command, cancellationToken);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(Unit.Value));
    }
}
