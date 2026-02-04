using CSharpFunctionalExtensions;
using Moq;

namespace VibraHeka.Infrastructure.UnitTests.Services.EmailTemplateStorageServiceTest;

[TestFixture]
[Category("unit")]
public class GetTemplateContentTest : GenericEmailTemplateStorageServiceTest
{
    [Test]
    public async Task ShouldCallRepositoryAndReturnContentWhenGetTemplateContentIsCalled()
    {
        // Given
        const string templateId = "template-123";
        const string expectedContent = "{\"template\":\"Hello\"}";

        RepositoryMock
            .Setup(r => r.GetTemplateContent(templateId, TestCancellationToken))
            .ReturnsAsync(Result.Success(expectedContent));
        
        RepositoryMock.Setup(r => r.TemplateExistsAsync(templateId, TestCancellationToken))
            .ReturnsAsync(Result.Success(true));

        // When
        Result<string> result = await Service.GetTemplateContent(templateId, TestCancellationToken);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedContent));
        RepositoryMock.Verify(r => r.GetTemplateContent(templateId, TestCancellationToken), Times.Once);
        RepositoryMock.Verify(r => r.TemplateExistsAsync(templateId, TestCancellationToken), Times.Once);
        RepositoryMock.VerifyNoOtherCalls();
    }

    [Test]
    public void ShouldThrowWhenRepositoryThrowsWhenGetTemplateContentIsCalled()
    {
        // Given
        const string templateId = "template-123";

        RepositoryMock
            .Setup(r => r.GetTemplateContent(templateId, TestCancellationToken))
            .ThrowsAsync(new IOException("Read failed"));

        // When / Then
        Assert.That(
            async () => await Service.GetTemplateContent(templateId, TestCancellationToken),
            Throws.Nothing);

        RepositoryMock.Verify(r => r.GetTemplateContent(templateId, TestCancellationToken), Times.Never);
        RepositoryMock.Verify(r => r.TemplateExistsAsync(templateId, TestCancellationToken), Times.Once);
        RepositoryMock.VerifyNoOtherCalls();
        
    }
}
