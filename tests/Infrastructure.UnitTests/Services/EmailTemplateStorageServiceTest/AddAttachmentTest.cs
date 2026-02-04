using System.Text;
using CSharpFunctionalExtensions;
using Moq;

namespace VibraHeka.Infrastructure.UnitTests.Services.EmailTemplateStorageServiceTest;

[TestFixture]
[Category("unit")]
public class AddAttachmentTest : GenericEmailTemplateStorageServiceTest
{
    [Test]
    public async Task ShouldCallRepositoryAndReturnResultWhenAddAttachmentIsCalled()
    {
        // Given: a repository success to verify the service forwards the call.
        string templateId = Guid.NewGuid().ToString("N");
        string attachmentName = "file.bin";
        byte[] bytes = Encoding.UTF8.GetBytes("payload");
        using MemoryStream attachmentStream = new MemoryStream(bytes);

        RepositoryMock
            .Setup(r => r.SaveAttachment(templateId, attachmentStream, attachmentName, TestCancellationToken))
            .ReturnsAsync(Result.Success("url"));

        // When: invoking the service method.
        Result<string> result = await Service.AddAttachment(templateId, attachmentStream, attachmentName, TestCancellationToken);

        // Then
        Assert.That(result.IsSuccess);
        Assert.That(result.Value, Is.EqualTo("url"));

        RepositoryMock.Verify(
            r => r.SaveAttachment(templateId, attachmentStream, attachmentName, TestCancellationToken),
            Times.Once);
        RepositoryMock.VerifyNoOtherCalls();
    }

    [Test]
    public void ShouldThrowWhenRepositoryThrowsWhenAddAttachmentIsCalled()
    {
        // Given: a repository exception to verify it propagates to the caller.
        string templateId = Guid.NewGuid().ToString("N");
        string attachmentName = "file.bin";
        byte[] bytes = Encoding.UTF8.GetBytes("payload");

        using (MemoryStream attachmentStream = new MemoryStream(bytes))
        {
            RepositoryMock
                .Setup(r => r.SaveAttachment(templateId, attachmentStream, attachmentName, TestCancellationToken))
                .ThrowsAsync(new IOException("Upload failed"));

            // When / Then: invoking the service should throw.
            Assert.That(
                async () => await Service.AddAttachment(templateId, attachmentStream, attachmentName, TestCancellationToken),
                Throws.TypeOf<IOException>());

            RepositoryMock.Verify(
                r => r.SaveAttachment(templateId, attachmentStream, attachmentName, TestCancellationToken),
                Times.Once);
            RepositoryMock.VerifyNoOtherCalls();
        }
    }
}
