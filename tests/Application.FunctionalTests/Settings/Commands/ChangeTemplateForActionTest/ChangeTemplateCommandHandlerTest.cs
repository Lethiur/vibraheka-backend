using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Settings.Commands.ChangeTemplateForAction;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Common.Interfaces.Settings;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Application.FunctionalTests.Settings.Commands.ChangeTemplateForActionTest;

[TestFixture]
public class ChangeTemplateCommandHandlerTest
{
    private Mock<ISettingsService> settingsServiceMock;
    private Mock<ICurrentUserService> CurrentUserServiceMock;
    private Mock<IPrivilegeService> PrivilegeServiceMock;
    private Mock<IEmailTemplatesService> EmailTemplatesServiceMock;
    private ChangeTemplateForActionCommandHandler Handler;

    [SetUp]
    public void SetUp()
    {
        settingsServiceMock = new Mock<ISettingsService>();
        CurrentUserServiceMock = new Mock<ICurrentUserService>();
        PrivilegeServiceMock = new Mock<IPrivilegeService>();
        EmailTemplatesServiceMock = new Mock<IEmailTemplatesService>();

        Handler = new ChangeTemplateForActionCommandHandler(
            settingsServiceMock.Object,
            CurrentUserServiceMock.Object,
            PrivilegeServiceMock.Object,
            EmailTemplatesServiceMock.Object);
    }

    [Test]
    public async Task ShouldHandleInvalidUserID()
    {
        // Arrange
        CurrentUserServiceMock.Setup(x => x.UserId).Returns(string.Empty);
        ChangeTemplateForActionCommand command = new ChangeTemplateForActionCommand("1", ActionType.UserVerification);

        // Act
        Result<Unit> result = await Handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidUserID));
    }

    [Test]
    public async Task ShouldReturnNotAuthorizedErrorIfUserIsNotAdmin()
    {
        // Arrange
        const string userId = "user-123";
        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        PrivilegeServiceMock.Setup(x => x.HasRoleAsync(userId, UserRole.Admin))
            .ReturnsAsync(false);

        ChangeTemplateForActionCommand command = new ChangeTemplateForActionCommand("1", ActionType.UserVerification);

        // Act
        Result<Unit> result = await Handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.NotAuthorized));
    }

    [Test]
    public async Task ShouldReturnFailureIfTemplateDoesNotExist()
    {
        // Arrange
        const string userId = "admin-123";
        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        PrivilegeServiceMock.Setup(x => x.HasRoleAsync(userId, UserRole.Admin))
            .ReturnsAsync(true);
        
        EmailTemplatesServiceMock.Setup(x => x.GetTemplateByID(It.IsAny<string>()))
            .ReturnsAsync(Result.Failure<EmailEntity>("Template not found"));

        ChangeTemplateForActionCommand command = new ChangeTemplateForActionCommand("99", ActionType.UserVerification);

        // Act
        Result<Unit> result = await Handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("Template not found"));
    }

    [Test]
    public async Task ShouldReturnInvalidActionErrorIfActionIsInvalid()
    {
        // Arrange
        const string userId = "admin-123";
        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        PrivilegeServiceMock.Setup(x => x.HasRoleAsync(userId, UserRole.Admin)).ReturnsAsync(true);
        EmailTemplatesServiceMock.Setup(x => x.GetTemplateByID("1")).ReturnsAsync(Result.Success(new EmailEntity()));

        // Usamos un cast o un valor no contemplado en el switch si el enum lo permite
        ChangeTemplateForActionCommand command = new ChangeTemplateForActionCommand("1", ActionType.PasswordReset);

        // Act
        Result<Unit> result = await Handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(EmailTemplateErrors.InvalidAction));
    }

    [Test]
    public async Task ShouldCallSettingsServiceAndReturnSuccessIfEverythingIsOk()
    {
        // Arrange
        const string userId = "admin-123";
        const string templateId = "1";
        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        PrivilegeServiceMock.Setup(x => x.HasRoleAsync(userId, UserRole.Admin)).ReturnsAsync(true);
        EmailTemplatesServiceMock.Setup(x => x.GetTemplateByID(templateId)).ReturnsAsync(Result.Success(new EmailEntity()));
        settingsServiceMock.Setup(x => x.ChangeEmailForVerificationAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Unit.Value));

        ChangeTemplateForActionCommand command = new ChangeTemplateForActionCommand(templateId, ActionType.UserVerification);

        // Act
        Result<Unit> result = await Handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        settingsServiceMock.Verify(x => x.ChangeEmailForVerificationAsync(templateId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
