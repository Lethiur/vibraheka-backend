using System.Text;
using CSharpFunctionalExtensions;
using Moq;

namespace VibraHeka.Infrastructure.UnitTests.Services.EmailTemplateStorageServiceTest;

[TestFixture]
[Category("unit")]
public class SaveTemplateTest : GenericEmailTemplateStorageServiceTest
{
    [Test]
    public async Task ShouldCallRepositoryAndReturnTemplateIdWhenSaveTemplateIsCalled()
    {
        // Given
        string templateId = Guid.NewGuid().ToString("N");
        byte[] bytes = Encoding.UTF8.GetBytes("""{"template":"Hello"}""");

        using MemoryStream templateStream = new MemoryStream(bytes);
        RepositoryMock
            .Setup(r => r.SaveTemplate(templateId, templateStream, TestCancellationToken))
            .ReturnsAsync(Result.Success(""));

        // When
        Result<string> result = await Service.SaveTemplate(templateId, templateStream, TestCancellationToken);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(""));

        RepositoryMock.Verify(r => r.SaveTemplate(templateId, templateStream, TestCancellationToken), Times.Once);
        RepositoryMock.VerifyNoOtherCalls();
    }

    [Test]
    public void ShouldThrowWhenRepositoryThrowsWhenSaveTemplateIsCalled()
    {
        // Given
        string templateId = Guid.NewGuid().ToString("N");
        byte[] bytes = Encoding.UTF8.GetBytes("""{"template":"Boom"}""");

        using (MemoryStream templateStream = new MemoryStream(bytes))
        {
            RepositoryMock
                .Setup(r => r.SaveTemplate(templateId, templateStream, TestCancellationToken))
                .ThrowsAsync(new IOException("Upload failed"));

            // When / Then
            Assert.That(
                async () => await Service.SaveTemplate(templateId, templateStream, TestCancellationToken),
                Throws.TypeOf<IOException>());

            RepositoryMock.Verify(r => r.SaveTemplate(templateId, templateStream, TestCancellationToken), Times.Once);
            RepositoryMock.VerifyNoOtherCalls();
        }
    }
}
