using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
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
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<IEmailTemplatesService> emailTemplatesServiceMock;
    private ChangeTemplateForActionCommandHandler handler;

    [SetUp]
    public void SetUp()
    {
        settingsServiceMock = new Mock<ISettingsService>();
        currentUserServiceMock = new Mock<ICurrentUserService>();
        emailTemplatesServiceMock = new Mock<IEmailTemplatesService>();

        handler = new ChangeTemplateForActionCommandHandler(
            settingsServiceMock.Object,
            currentUserServiceMock.Object,
            emailTemplatesServiceMock.Object, NullLogger<ChangeTemplateForActionCommandHandler>.Instance);
    }

    [Test]
    public async Task ShouldHandleInvalidUserID()
    {
        currentUserServiceMock.Setup(x => x.UserId).Returns(string.Empty);
        ChangeTemplateForActionCommand command = new("1", ActionType.UserVerification);

        Result<Unit> result = await handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidUserID));
    }

    [Test]
    public async Task ShouldReturnFailureIfTemplateDoesNotExist()
    {
        currentUserServiceMock.Setup(x => x.UserId).Returns("admin-123");
        emailTemplatesServiceMock.Setup(x => x.GetTemplateByID(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(Result.Failure<EmailEntity>("Template not found"));

        ChangeTemplateForActionCommand command = new("99", ActionType.UserVerification);
        Result<Unit> result = await handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("Template not found"));
    }

    [Test]
    public async Task ShouldCallVerificationSettingsServiceBranch()
    {
        const string templateId = "1";
        currentUserServiceMock.Setup(x => x.UserId).Returns("admin-123");
        emailTemplatesServiceMock.Setup(x => x.GetTemplateByID(templateId, CancellationToken.None))
            .ReturnsAsync(Result.Success(new EmailEntity()));
        settingsServiceMock.Setup(x => x.ChangeEmailForVerificationAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Unit.Value));

        ChangeTemplateForActionCommand command = new(templateId, ActionType.UserVerification);
        Result<Unit> result = await handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        settingsServiceMock.Verify(x => x.ChangeEmailForVerificationAsync(templateId, It.IsAny<CancellationToken>()), Times.Once);
        settingsServiceMock.Verify(x => x.ChangeRecoverPasswordEmailTemplateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ShouldCallPasswordResetSettingsServiceBranch()
    {
        const string templateId = "1";
        currentUserServiceMock.Setup(x => x.UserId).Returns("admin-123");
        emailTemplatesServiceMock.Setup(x => x.GetTemplateByID(templateId, CancellationToken.None))
            .ReturnsAsync(Result.Success(new EmailEntity()));
        settingsServiceMock.Setup(x => x.ChangeRecoverPasswordEmailTemplateAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Unit.Value));

        ChangeTemplateForActionCommand command = new(templateId, ActionType.PasswordReset);
        Result<Unit> result = await handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        settingsServiceMock.Verify(x => x.ChangeRecoverPasswordEmailTemplateAsync(templateId, It.IsAny<CancellationToken>()), Times.Once);
        settingsServiceMock.Verify(x => x.ChangeEmailForVerificationAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ShouldReturnInvalidActionErrorIfActionIsInvalid()
    {
        const string templateId = "1";
        currentUserServiceMock.Setup(x => x.UserId).Returns("admin-123");
        emailTemplatesServiceMock.Setup(x => x.GetTemplateByID(templateId, CancellationToken.None))
            .ReturnsAsync(Result.Success(new EmailEntity()));

        ChangeTemplateForActionCommand command = new(templateId, (ActionType)999);
        Result<Unit> result = await handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(EmailTemplateErrors.InvalidAction));
    }

    [Test]
    [TestCase(ActionType.UserRegistered)]
    [TestCase(ActionType.SubscriptionThankYou)]
    [TestCase(ActionType.TrialEndingSoon)]
    [TestCase(ActionType.PasswordChanged)]
    public async Task ShouldHandleNewAdminTemplateActions(ActionType actionType)
    {
        // Given
        const string templateId = "1";
        currentUserServiceMock.Setup(x => x.UserId).Returns("admin-123");
        emailTemplatesServiceMock.Setup(x => x.GetTemplateByID(templateId, CancellationToken.None))
            .ReturnsAsync(Result.Success(new EmailEntity()));

        settingsServiceMock.Setup(x => x.ChangeUserWelcomeEmailTemplateAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Unit.Value));
        settingsServiceMock.Setup(x => x.ChangeSubscriptionThankYouEmailTemplateAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Unit.Value));
        settingsServiceMock.Setup(x => x.ChangeTrialEndingSoonEmailTemplateAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Unit.Value));
        settingsServiceMock.Setup(x => x.ChangePasswordChangedEmailTemplateAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Unit.Value));

        // When
        ChangeTemplateForActionCommand command = new(templateId, actionType);
        Result<Unit> result = await handler.Handle(command, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
    }
}
