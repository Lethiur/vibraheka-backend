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
    private Mock<ICurrentUserService> _currentUserServiceMock;
    private Mock<IPrivilegeService> _privilegeServiceMock;
    private Mock<IEmailTemplateStorageService> _storageServiceMock;
    private Mock<IEmailTemplatesService> _templateServiceMock;
    private CreateEmailTemplateCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _privilegeServiceMock = new Mock<IPrivilegeService>();
        _storageServiceMock = new Mock<IEmailTemplateStorageService>();
        _templateServiceMock = new Mock<IEmailTemplatesService>();

        _handler = new CreateEmailTemplateCommandHandler(
            _currentUserServiceMock.Object,
            _templateServiceMock.Object,
            _storageServiceMock.Object,
            _privilegeServiceMock.Object
        );
    }

    [Test]
    [Description(
        "Given a valid admin user with a valid email template file, when creating an email template, then the template should be saved successfully")]
    public async Task ShouldCreateEmailTemplateWhenUserIsAdminAndRequestIsValid()
    {
        // Given
        string userId = "user-123";
        string templateName = "Welcome Email";
        MemoryStream fileStream = new MemoryStream(Encoding.UTF8.GetBytes("{a: 3}"));
        CancellationToken cancellationToken = CancellationToken.None;
        CreateEmailTemplateCommand command = new CreateEmailTemplateCommand(fileStream, templateName);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _privilegeServiceMock.Setup(x => x.HasRoleAsync(userId, UserRole.Admin))
            .ReturnsAsync(true);
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
        "Given a user with null user ID, when creating an email template, then it should return InvalidUserID error")]
    public async Task ShouldReturnInvalidUserIdErrorWhenUserIdIsNull()
    {
        // Given
        const string templateName = "Welcome Email";
        MemoryStream fileStream = new MemoryStream(Encoding.UTF8.GetBytes("{a: 3}"));
        CancellationToken cancellationToken = CancellationToken.None;
        CreateEmailTemplateCommand command = new CreateEmailTemplateCommand(fileStream, templateName);

        _currentUserServiceMock.Setup(x => x.UserId).Returns((string)null!);

        // When
        Result<Unit> result = await _handler.Handle(command, cancellationToken);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidUserID));
        _privilegeServiceMock.Verify(
            x => x.HasRoleAsync(It.IsAny<string>(), It.IsAny<UserRole>()), Times.Never);
    }

    [Test]
    [Description(
        "Given a user with empty user ID, when creating an email template, then it should return InvalidUserID error")]
    public async Task ShouldReturnInvalidUserIdErrorWhenUserIdIsEmpty()
    {
        // Given
        const string templateName = "Welcome Email";
        MemoryStream fileStream = new MemoryStream(Encoding.UTF8.GetBytes("Template content"));
        CancellationToken cancellationToken = CancellationToken.None;
        CreateEmailTemplateCommand command = new CreateEmailTemplateCommand(fileStream, templateName);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(string.Empty);

        // When
        Result<Unit> result = await _handler.Handle(command, cancellationToken);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidUserID));
    }

    [Test]
    [Description(
        "Given a user with whitespace-only user ID, when creating an email template, then it should return InvalidUserID error")]
    public async Task ShouldReturnInvalidUserIdErrorWhenUserIdIsWhitespace()
    {
        // Given
        const string templateName = "Welcome Email";
        MemoryStream fileStream = new MemoryStream(Encoding.UTF8.GetBytes("Template content"));
        CancellationToken cancellationToken = CancellationToken.None;
        CreateEmailTemplateCommand command = new CreateEmailTemplateCommand(fileStream, templateName);

        _currentUserServiceMock.Setup(x => x.UserId).Returns("   ");

        // When
        Result<Unit> result = await _handler.Handle(command, cancellationToken);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidUserID));
    }

    [Test]
    [Description(
        "Given a user without admin role, when creating an email template, then it should return NotAuthorized error")]
    public async Task ShouldReturnNotAuthorizedErrorWhenUserDoesNotHaveAdminRole()
    {
        // Given
        string userId = "user-123";
        const string templateName = "Welcome Email";
        MemoryStream fileStream = new MemoryStream(Encoding.UTF8.GetBytes("Template content"));
        CancellationToken cancellationToken = CancellationToken.None;
        CreateEmailTemplateCommand command = new CreateEmailTemplateCommand(fileStream, templateName);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _privilegeServiceMock.Setup(x => x.HasRoleAsync(userId, UserRole.Admin)).ReturnsAsync(false);

        // When
        Result<Unit> result = await _handler.Handle(command, cancellationToken);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.NotAuthorized));
        _storageServiceMock.Verify(
            x => x.SaveTemplate(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    [Description(
        "Given a valid admin user but storage service fails, when creating an email template, then it should return storage error")]
    public async Task ShouldReturnStorageErrorWhenStorageServiceFails()
    {
        // Given
        string userId = "user-123";
        const string templateName = "Welcome Email";
        MemoryStream fileStream = new MemoryStream(Encoding.UTF8.GetBytes("Template content"));
        CancellationToken cancellationToken = CancellationToken.None;
        CreateEmailTemplateCommand command = new CreateEmailTemplateCommand(fileStream, templateName);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _privilegeServiceMock.Setup(x => x.HasRoleAsync(userId, UserRole.Admin)).ReturnsAsync(true);
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
        const string userId = "user-123";

        const string templateName = "Welcome Email";
        MemoryStream fileStream = new MemoryStream(Encoding.UTF8.GetBytes("Template content"));
        CancellationToken cancellationToken = CancellationToken.None;
        CreateEmailTemplateCommand command = new CreateEmailTemplateCommand(fileStream, templateName);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _privilegeServiceMock.Setup(x => x.HasRoleAsync(userId, UserRole.Admin)).ReturnsAsync(true);
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
        string userId = "user-123";
        const string templateName = "Welcome Email";
        MemoryStream fileStream = new MemoryStream(Encoding.UTF8.GetBytes("Template content"));
        CancellationToken cancellationToken = CancellationToken.None;
        CreateEmailTemplateCommand command = new CreateEmailTemplateCommand(fileStream, templateName);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _privilegeServiceMock.Setup(x => x.HasRoleAsync(userId, UserRole.Admin)).ReturnsAsync(true);
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

    [TestCase(null!)]
    [TestCase("")]
    [TestCase("   ")]
    [Description(
        "Given a user with various invalid user ID formats, when creating an email template, then it should return InvalidUserID error")]
    public async Task ShouldReturnInvalidUserIdErrorWhenUserIdHasInvalidFormat(string userId)
    {
        // Given
        const string templateName = "Welcome Email";
        MemoryStream fileStream = new MemoryStream(Encoding.UTF8.GetBytes("Template content"));
        CancellationToken cancellationToken = CancellationToken.None;
        CreateEmailTemplateCommand command = new CreateEmailTemplateCommand(fileStream, templateName);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        // When
        Result<Unit> result = await _handler.Handle(command, cancellationToken);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidUserID));
    }

    [Test]
    [Description(
        "Given a valid admin user creating an email template, when the storage service is called, then it should be called exactly once with the correct parameters")]
    public async Task ShouldCallStorageServiceOnceWhenCreatingEmailTemplate()
    {
        // Given
        string userId = "user-123";
        const string templateName = "Welcome Email";
        MemoryStream fileStream = new MemoryStream(Encoding.UTF8.GetBytes("Template content"));
        CancellationToken cancellationToken = CancellationToken.None;
        CreateEmailTemplateCommand command = new CreateEmailTemplateCommand(fileStream, templateName);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _privilegeServiceMock.Setup(x => x.HasRoleAsync(userId, UserRole.Admin)).ReturnsAsync(true);
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
        string userId = "user-123";
        const string templateName = "Welcome Email";
        MemoryStream fileStream = new MemoryStream(Encoding.UTF8.GetBytes("Template content"));
        CancellationToken cancellationToken = CancellationToken.None;
        CreateEmailTemplateCommand command = new CreateEmailTemplateCommand(fileStream, templateName);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _privilegeServiceMock.Setup(x => x.HasRoleAsync(userId, UserRole.Admin))
            .ReturnsAsync(true);
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
