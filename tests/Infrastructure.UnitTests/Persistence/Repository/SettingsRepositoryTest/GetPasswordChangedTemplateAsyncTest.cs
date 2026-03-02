using Amazon.SimpleSystemsManagement.Model;
using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.SettingsRepositoryTest;

[TestFixture]
public class GetRecoverPasswordEmailTemplateAsyncTest : GenericSettingsRepositoryTest
{
    [Test]
    public async Task ShouldReturnRecoverPasswordEmailTemplateWhenParameterExists()
    {
        const string expectedValue = "<html>Password changed template</html>";
        GetParameterResponse response = new()
        {
            Parameter = new Parameter { Value = expectedValue }
        };

        SsmClientMock.Setup(x => x.GetParameterAsync(
                It.Is<GetParameterRequest>(r => r.Name == PasswordChangedParameterName),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        Result<string> result = await Repository.GetRecoverPasswordEmailTemplateAsync();

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedValue));
    }

    [Test]
    public async Task ShouldReturnInfrastructureParameterNotFoundWhenRecoverPasswordEmailTemplateDoesNotExist()
    {
        SsmClientMock.Setup(x => x.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ParameterNotFoundException("Not found"));

        Result<string> result = await Repository.GetRecoverPasswordEmailTemplateAsync();

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(InfrastructureConfigErrors.ParameterNotFound));
    }

    [Test]
    public async Task ShouldReturnGenericErrorWhenUnexpectedExceptionOccursWhileGettingRecoverPasswordEmailTemplate()
    {
        SsmClientMock.Setup(x => x.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unknown crash"));

        Result<string> result = await Repository.GetRecoverPasswordEmailTemplateAsync();

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(AppErrors.GenericError));
    }
}
