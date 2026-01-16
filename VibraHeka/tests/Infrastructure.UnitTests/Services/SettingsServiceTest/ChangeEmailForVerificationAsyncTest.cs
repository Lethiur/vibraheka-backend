using System.ComponentModel;
using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Interfaces.Settings;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.UnitTests.Services.SettingsServiceTest;

public class ChangeEmailForVerificationAsyncTest
{
     private Mock<ISettingsRepository> _repositoryMock;
    private SettingsService _service;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<ISettingsRepository>();
        _service = new SettingsService(_repositoryMock.Object);
    }

    [Test]
    [DisplayName("Should return success when a valid email template is provided")]
    public async Task ShouldReturnSuccessWhenValidEmailTemplateProvided()
    {
        // Given: A valid email template string
        const string newTemplate = "<html>Verification Code: {{code}}</html>";
        CancellationToken cancellationToken = CancellationToken.None;

        _repositoryMock.Setup(x => x.UpdateVerificationEmailTemplateAsync(newTemplate, cancellationToken))
            .ReturnsAsync(Result.Success(Unit.Value));

        // When: Changing the email verification template
        Result<Unit> result = await _service.ChangeEmailForVerificationAsync(newTemplate, cancellationToken);

        // Then: The operation should be successful
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(Unit.Value));
        _repositoryMock.Verify(x => x.UpdateVerificationEmailTemplateAsync(newTemplate, cancellationToken), Times.Once);
    }

    [Test]
    [DisplayName("Should return InvalidVerificationEmailTemplate when template is null or whitespace")]
    [TestCase(null!)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task ShouldReturnFailureWhenTemplateIsInvalid(string invalidTemplate)
    {
        // Given: An invalid template input

        // When: Attempting to change the email template
        Result<Unit> result = await _service.ChangeEmailForVerificationAsync(invalidTemplate, CancellationToken.None);

        // Then: Should fail with the specific settings error and not call the repository
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SettingsErrors.InvalidVerificationEmailTemplate));
        _repositoryMock.Verify(x => x.UpdateVerificationEmailTemplateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    [DisplayName("Should propagate failure when repository update fails")]
    public async Task ShouldPropagateFailureWhenRepositoryFails()
    {
        // Given: A valid template but the repository fails to save
        const string template = "Valid Template";
        const string repoErrorMessage = "Database connection error";
        
        _repositoryMock.Setup(x => x.UpdateVerificationEmailTemplateAsync(template, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Unit>(repoErrorMessage));

        // When: Attempting to change the email template
        Result<Unit> result = await _service.ChangeEmailForVerificationAsync(template, CancellationToken.None);

        // Then: The service should return the repository's error
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(repoErrorMessage));
    }
}
