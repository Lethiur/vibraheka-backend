using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.SettingsRepositoryTest;

[TestFixture]
public class GetVerificationEmailTemplateAsyncTest : GenericSettingsRepositoryTest
{
    [Test]
    public async Task ShouldReturnTemplateWhenParameterExists()
    {
        const string expectedValue = "<html>template</html>";
        GetParameterResponse response = new()
        {
            Parameter = new Parameter { Value = expectedValue }
        };

        SsmClientMock.Setup(x => x.GetParameterAsync(
                It.Is<GetParameterRequest>(r => r.Name == VerificationParameterName),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        Result<string> result = await Repository.GetVerificationEmailTemplateAsync();

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedValue));
    }

    [Test]
    public async Task ShouldReturnParameterNotFoundWhenAwsThrowsParameterNotFoundException()
    {
        SsmClientMock.Setup(x => x.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ParameterNotFoundException("Not found"));

        Result<string> result = await Repository.GetVerificationEmailTemplateAsync();

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(InfrastructureConfigErrors.ParameterNotFound));
    }

    [Test]
    public async Task ShouldReturnAccessDeniedWhenAwsThrowsAmazonSimpleSystemsManagementException()
    {
        SsmClientMock.Setup(x => x.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonSimpleSystemsManagementException("Access denied"));

        Result<string> result = await Repository.GetVerificationEmailTemplateAsync();

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(InfrastructureConfigErrors.AccessDenied));
    }

    [Test]
    public async Task ShouldReturnGenericErrorWhenAwsThrowsGenericException()
    {
        SsmClientMock.Setup(x => x.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unknown crash"));

        Result<string> result = await Repository.GetVerificationEmailTemplateAsync();

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(AppErrors.GenericError));
    }
}
