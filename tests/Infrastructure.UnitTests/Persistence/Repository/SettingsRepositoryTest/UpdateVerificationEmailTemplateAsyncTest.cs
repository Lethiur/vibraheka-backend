using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.SettingsRepositoryTest;

[TestFixture]
public class UpdateVerificationEmailTemplateAsyncTest : GenericSettingsRepositoryTest
{
    [Test]
    public async Task ShouldReturnSuccessWhenParameterIsUpdatedCorrectly()
    {
        const string emailTemplate = "<html><body>Verify</body></html>";

        SsmClientMock.Setup(x => x.PutParameterAsync(
                It.Is<PutParameterRequest>(r => r.Name == VerificationParameterName && r.Value == emailTemplate),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutParameterResponse());

        Result<Unit> result = await Repository.UpdateVerificationEmailTemplateAsync(emailTemplate, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(Unit.Value));
    }

    [Test]
    public async Task ShouldReturnParameterLimitExceededWhenAwsThrowsParameterLimitExceededException()
    {
        SsmClientMock.Setup(x => x.PutParameterAsync(It.IsAny<PutParameterRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ParameterLimitExceededException("Limit reached"));

        Result<Unit> result = await Repository.UpdateVerificationEmailTemplateAsync("template", CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(InfrastructureConfigErrors.ParameterLimitExceeded));
    }

    [Test]
    public async Task ShouldReturnTooManyUpdatesWhenAwsThrowsTooManyUpdatesException()
    {
        SsmClientMock.Setup(x => x.PutParameterAsync(It.IsAny<PutParameterRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TooManyUpdatesException("Too many updates"));

        Result<Unit> result = await Repository.UpdateVerificationEmailTemplateAsync("template", CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(InfrastructureConfigErrors.TooManyUpdates));
    }

    [Test]
    public async Task ShouldReturnAccessDeniedWhenAwsThrowsAmazonSimpleSystemsManagementException()
    {
        SsmClientMock.Setup(x => x.PutParameterAsync(It.IsAny<PutParameterRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonSimpleSystemsManagementException("Access denied"));

        Result<Unit> result = await Repository.UpdateVerificationEmailTemplateAsync("template", CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(InfrastructureConfigErrors.AccessDenied));
    }

    [Test]
    public async Task ShouldReturnGenericErrorWhenGeneralExceptionIsThrown()
    {
        SsmClientMock.Setup(x => x.PutParameterAsync(It.IsAny<PutParameterRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        Result<Unit> result = await Repository.UpdateVerificationEmailTemplateAsync("template", CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(AppErrors.GenericError));
    }
}
