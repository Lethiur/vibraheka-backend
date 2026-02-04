using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Infrastructure.UnitTests.Services.EmailTemplateStorageServiceTest;

[TestFixture]
[Category("unit")]
public class GetTemplateUrlAsyncTest : GenericEmailTemplateStorageServiceTest
{
    [Test]
    public async Task ShouldReturnUrlWhenTemplateExists()
    {
        // Given
        const string templateId = "template-123";
        const string expectedUrl = "https://bucket.s3.eu-west-1.amazonaws.com/template-123/template.json";

        RepositoryMock
            .Setup(r => r.TemplateExistsAsync(templateId, TestCancellationToken))
            .ReturnsAsync(Result.Success(true));

        RepositoryMock
            .Setup(r => r.GetTemplateUrlAsync(templateId))
            .ReturnsAsync(Result.Success(expectedUrl));

        // When
        Result<string> result = await Service.GetTemplateUrlAsync(templateId, TestCancellationToken);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedUrl));

        RepositoryMock.Verify(r => r.TemplateExistsAsync(templateId, TestCancellationToken), Times.AtLeastOnce);
        RepositoryMock.Verify(r => r.GetTemplateUrlAsync(templateId), Times.Once);
        RepositoryMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ShouldReturnTemplateNotFoundWhenTemplateDoesNotExist()
    {
        // Given
        const string templateId = "missing-template";

        RepositoryMock
            .Setup(r => r.TemplateExistsAsync(templateId, TestCancellationToken))
            .ReturnsAsync(Result.Success(false));

        // When
        Result<string> result = await Service.GetTemplateUrlAsync(templateId, TestCancellationToken);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(EmailTemplateErrors.TemplateNotFound));

        RepositoryMock.Verify(r => r.TemplateExistsAsync(templateId, TestCancellationToken), Times.AtLeastOnce);
        RepositoryMock.Verify(r => r.GetTemplateUrlAsync(It.IsAny<string>()), Times.Never);
        RepositoryMock.VerifyNoOtherCalls();
    }
}
