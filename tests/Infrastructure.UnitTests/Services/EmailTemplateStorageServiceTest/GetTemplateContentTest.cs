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

        // When
        Result<string> result = await Service.GetTemplateContent(templateId, TestCancellationToken);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedContent));
        RepositoryMock.Verify(r => r.GetTemplateContent(templateId, TestCancellationToken), Times.Once);
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
            Throws.TypeOf<IOException>());

        RepositoryMock.Verify(r => r.GetTemplateContent(templateId, TestCancellationToken), Times.Once);
        RepositoryMock.VerifyNoOtherCalls();
    }
}
