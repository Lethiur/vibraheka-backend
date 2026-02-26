using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.UnitTests.Services.SettingsServiceTest;

[TestFixture]
public class ChangeEmailForResetPasswordAsyncTest : GenericSettingsServiceTest
{
    [Test]
    public async Task ShouldReturnSuccessWhenValidResetPasswordTemplateProvided()
    {
        const string newTemplate = "<html>Password changed</html>";

        RepositoryMock.Setup(x => x.UpdatePasswordChangedTemplateAsync(newTemplate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Unit.Value));

        Result<Unit> result = await Service.ChangeEmailForResetPasswordAsync(newTemplate, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        RepositoryMock.Verify(x => x.UpdatePasswordChangedTemplateAsync(newTemplate, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestCase(null!)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task ShouldReturnInvalidPasswordChangedTemplateWhenTemplateIsNullOrWhitespace(string invalidTemplate)
    {
        Result<Unit> result = await Service.ChangeEmailForResetPasswordAsync(invalidTemplate, CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SettingsErrors.InvalidPasswordChangedTemplate));
        RepositoryMock.Verify(x => x.UpdatePasswordChangedTemplateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task ShouldMapInfrastructureErrorToPasswordChangedDomainError()
    {
        RepositoryMock.Setup(x => x.UpdatePasswordChangedTemplateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Unit>(InfrastructureConfigErrors.TooManyUpdates));

        Result<Unit> result = await Service.ChangeEmailForResetPasswordAsync("template", CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SettingsErrors.PasswordChangedTemplateUpdateFailed));
    }
}
