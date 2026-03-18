using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.UnitTests.Services.SettingsServiceTest;

[TestFixture]
public class ChangeRecoverPasswordEmailTemplateAsyncTest : GenericSettingsServiceTest
{
    [Test]
    public async Task ShouldReturnSuccessWhenValidResetPasswordTemplateProvided()
    {
        const string newTemplate = "<html>Password changed</html>";

        RepositoryMock.Setup(x => x.UpdateRecoverPasswordEmailTemplateAsync(newTemplate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Unit.Value));

        Result<Unit> result = await Service.ChangeRecoverPasswordEmailTemplateAsync(newTemplate, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        RepositoryMock.Verify(x => x.UpdateRecoverPasswordEmailTemplateAsync(newTemplate, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestCase(null!)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task ShouldReturnInvalidRecoverPasswordEmailTemplateWhenTemplateIsNullOrWhitespace(string invalidTemplate)
    {
        Result<Unit> result = await Service.ChangeRecoverPasswordEmailTemplateAsync(invalidTemplate, CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SettingsErrors.InvalidRecoverPasswordEmailTemplate));
        RepositoryMock.Verify(x => x.UpdateRecoverPasswordEmailTemplateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task ShouldMapInfrastructureErrorToPasswordChangedDomainError()
    {
        RepositoryMock.Setup(x => x.UpdateRecoverPasswordEmailTemplateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Unit>(InfrastructureConfigErrors.TooManyUpdates));

        Result<Unit> result = await Service.ChangeRecoverPasswordEmailTemplateAsync("template", CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SettingsErrors.RecoverPasswordEmailTemplateUpdateFailed));
    }
    
    [Test]
    public async Task ShouldMapUnknownInfrastructureErrorToGenericDomainError()
    {
        // Given: Some mocking
        RepositoryMock.Setup(x => x.UpdateRecoverPasswordEmailTemplateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));
        
        // When: Service is invoked
        Result<Unit> result = await Service.ChangeRecoverPasswordEmailTemplateAsync("template", CancellationToken.None);
        
        // Then: Should return failure
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SettingsErrors.GenericError));
    }
}
