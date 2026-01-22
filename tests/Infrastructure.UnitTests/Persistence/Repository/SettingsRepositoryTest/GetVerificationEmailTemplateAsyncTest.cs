using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.SettingsRepositoryTest;

[TestFixture]
public class GetVerificationEmailTemplateAsyncTest
{
     private Mock<IAmazonSimpleSystemsManagement> SsmClientMock;
    private SettingsRepository Repository;
    private const string ParameterName = "/VibraHeka/VerificationEmailTemplate";

    [SetUp]
    public void SetUp()
    {
        SsmClientMock = new Mock<IAmazonSimpleSystemsManagement>();
        Repository = new SettingsRepository(SsmClientMock.Object);
    }

    [Test]
    public async Task ShouldReturnTemplateWhenParameterExists()
    {
        // Given
        const string expectedValue = "<html>template</html>";
        GetParameterResponse response = new GetParameterResponse
        {
            Parameter = new Parameter { Value = expectedValue }
        };

        SsmClientMock.Setup(x => x.GetParameterAsync(
                It.Is<GetParameterRequest>(r => r.Name == ParameterName), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // When
        Result<string> result = await Repository.GetVerificationEmailTemplateAsync();

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedValue));
    }

    [Test]
    public async Task ShouldReturnParameterNotFoundWhenAwsThrowsParameterNotFoundException()
    {
        // Given
        SsmClientMock.Setup(x => x.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ParameterNotFoundException("Not found"));

        // When
        Result<string> result = await Repository.GetVerificationEmailTemplateAsync();

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SettingsErrors.InvalidVerificationEmailTemplate));
    }

    [Test]
    public async Task ShouldReturnAccessDeniedWhenAwsThrowsAmazonSimpleSystemsManagementException()
    {
        // Given
        SsmClientMock.Setup(x => x.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonSimpleSystemsManagementException("Access Denied"));

        // When
        Result<string> result = await Repository.GetVerificationEmailTemplateAsync();

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.NotAuthorized));
    }

    [Test]
    public async Task ShouldReturnUnexpectedErrorWhenAwsThrowsGenericException()
    {
        // Given
        SsmClientMock.Setup(x => x.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unknown crash"));

        // When
        Result<string> result = await Repository.GetVerificationEmailTemplateAsync();

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(InfrastructureConfigErrors.UnexpectedError));
    }

    [Test]
    public async Task ShouldReturnSuccessWhenUpdatingParameterCorrectly()
    {
        // Given
        const string newTemplate = "new-template";
        SsmClientMock.Setup(x => x.PutParameterAsync(
                It.Is<PutParameterRequest>(r => r.Name == ParameterName && r.Value == newTemplate), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutParameterResponse());

        // When
        Result<MediatR.Unit> result = await Repository.UpdateVerificationEmailTemplateAsync(newTemplate, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        SsmClientMock.Verify(x => x.PutParameterAsync(It.IsAny<PutParameterRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
