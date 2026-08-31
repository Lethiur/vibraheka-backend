using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.UnitTests.Services.SettingsServiceTest;

[TestFixture]
public class ChangeEmailForVerificationAsyncTest : GenericSettingsServiceTest
{
    [Test]
    public async Task ShouldReturnSuccessWhenValidTemplateProvided()
    {
        Guid newTemplate = Guid.NewGuid();

        RepositoryMock.Setup(x => x.UpdateVerificationEmailTemplateAsync(newTemplate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Unit.Value));

        Result<Unit> result = await Service.ChangeEmailForVerificationAsync(newTemplate, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        RepositoryMock.Verify(x => x.UpdateVerificationEmailTemplateAsync(newTemplate, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ShouldReturnInvalidVerificationTemplateWhenTemplateIsNullOrWhitespace()
    {
        Result<Unit> result = await Service.ChangeEmailForVerificationAsync(Guid.Empty, CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SettingsErrors.InvalidVerificationEmailTemplate));
        RepositoryMock.Verify(x => x.UpdateVerificationEmailTemplateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ShouldMapInfrastructureErrorToDomainErrorWhenRepositoryFailsWithParameterLimitExceeded()
    {
        RepositoryMock.Setup(x => x.UpdateVerificationEmailTemplateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Unit>(InfrastructureConfigErrors.ParameterLimitExceeded));

        Result<Unit> result = await Service.ChangeEmailForVerificationAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SettingsErrors.VerificationEmailTemplateUpdateFailed));
    }

    [Test]
    public async Task ShouldMapUnknownInfrastructureErrorToGenericDomainError()
    {
        RepositoryMock.Setup(x => x.UpdateVerificationEmailTemplateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Unit>(AppErrors.GenericError));

        Result<Unit> result = await Service.ChangeEmailForVerificationAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SettingsErrors.GenericError));
    }

    [Test]
    public async Task ShouldHandleExceptionsFromRepository()
    {
        // Given: Some mocking
        RepositoryMock.Setup(x => x.UpdateVerificationEmailTemplateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // When: Service is invoked
        Result<Unit> result = await Service.ChangeEmailForVerificationAsync(Guid.NewGuid(), CancellationToken.None);

        // Then: Should return failure
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SettingsErrors.GenericError));
    }
}
