using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.UnitTests.Services.SettingsServiceTest;

[TestFixture]
public class GetTemplatesAsyncTest : GenericSettingsServiceTest
{
    [Test]
    public async Task ShouldReturnVerificationTemplateWhenRepositoryReturnsSuccess()
    {
        const string template = "verification-template-id";
        RepositoryMock.Setup(x => x.GetVerificationEmailTemplateAsync())
            .ReturnsAsync(Result.Success(template));

        Result<string> result = await Service.GetVerificationEmailTemplateAsync(CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(template));
    }

    [Test]
    public async Task ShouldMapVerificationParameterNotFoundToDomainError()
    {
        RepositoryMock.Setup(x => x.GetVerificationEmailTemplateAsync())
            .ReturnsAsync(Result.Failure<string>(InfrastructureConfigErrors.ParameterNotFound));

        Result<string> result = await Service.GetVerificationEmailTemplateAsync(CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SettingsErrors.InvalidVerificationEmailTemplate));
    }

    [Test]
    public async Task ShouldReturnRecoverPasswordEmailTemplateWhenRepositoryReturnsSuccess()
    {
        const string template = "password-changed-template-id";
        RepositoryMock.Setup(x => x.GetRecoverPasswordEmailTemplateAsync())
            .ReturnsAsync(Result.Success(template));

        Result<string> result = await Service.GetRecoverPasswordEmailTemplateAsync(CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(template));
    }

    [Test]
    public async Task ShouldMapPasswordChangedParameterNotFoundToDomainError()
    {
        RepositoryMock.Setup(x => x.GetRecoverPasswordEmailTemplateAsync())
            .ReturnsAsync(Result.Failure<string>(InfrastructureConfigErrors.ParameterNotFound));

        Result<string> result = await Service.GetRecoverPasswordEmailTemplateAsync(CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SettingsErrors.InvalidRecoverPasswordEmailTemplate));
    }

    [Test]
    public void ShouldThrowWhenVerificationTemplateReadIsCancelled()
    {
        using CancellationTokenSource cts = new();
        cts.Cancel();

        Assert.ThrowsAsync<OperationCanceledException>(() => Service.GetVerificationEmailTemplateAsync(cts.Token));
    }

    [Test]
    public void ShouldThrowWhenRecoverPasswordEmailTemplateReadIsCancelled()
    {
        using CancellationTokenSource cts = new();
        cts.Cancel();

        Assert.ThrowsAsync<OperationCanceledException>(() => Service.GetRecoverPasswordEmailTemplateAsync(cts.Token));
    }
}
