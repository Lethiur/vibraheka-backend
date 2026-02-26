using Amazon.SimpleSystemsManagement.Model;
using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.SettingsRepositoryTest;

[TestFixture]
public class UpdatePasswordChangedTemplateAsyncTest : GenericSettingsRepositoryTest
{
    [Test]
    public async Task ShouldReturnSuccessWhenPasswordChangedTemplateIsUpdatedCorrectly()
    {
        const string emailTemplate = "<html><body>Password changed</body></html>";

        SsmClientMock.Setup(x => x.PutParameterAsync(
                It.Is<PutParameterRequest>(r => r.Name == PasswordChangedParameterName && r.Value == emailTemplate),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutParameterResponse());

        Result<Unit> result = await Repository.UpdatePasswordChangedTemplateAsync(emailTemplate, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(Unit.Value));
    }

    [Test]
    public async Task ShouldReturnGenericErrorWhenUpdatingPasswordChangedTemplateThrowsUnexpectedException()
    {
        SsmClientMock.Setup(x => x.PutParameterAsync(It.IsAny<PutParameterRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        Result<Unit> result = await Repository.UpdatePasswordChangedTemplateAsync("template", CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(AppErrors.GenericError));
    }

    [Test]
    public async Task ShouldReturnMappedInfrastructureErrorWhenUpdatingPasswordChangedTemplateFailsByRateLimit()
    {
        SsmClientMock.Setup(x => x.PutParameterAsync(It.IsAny<PutParameterRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TooManyUpdatesException("Too many updates"));

        Result<Unit> result = await Repository.UpdatePasswordChangedTemplateAsync("template", CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(InfrastructureConfigErrors.TooManyUpdates));
    }
}
