using System.Text;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using Moq;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.S3.EmailTemplateStorageRepositoryTest;

public class SaveAttachmentTest : GenericEmailTemplateStorageRepositoryTest
{
    [Test]
    [Description("Given a valid attachment stream, when saving the attachment, then it should upload the temp file and delete it")]
    public async Task ShouldUploadTempFileAndDeleteItWhenSaveAttachmentIsCalled()
    {
        // Given: a valid stream with a non-zero offset to verify upload and cleanup.
        string templateId = Guid.NewGuid().ToString("N");
        string attachmentName = $"{Guid.NewGuid():N}.bin";
        byte[] expectedBytes = Encoding.UTF8.GetBytes("attachment-data");
        using MemoryStream attachmentStream = new MemoryStream(expectedBytes);
        attachmentStream.Position = 2;

        CancellationToken cancellationToken = CancellationToken.None;
        PutObjectRequest? capturedRequest = null;
        long? capturedLength = null;

        ClientMock
            .Setup(c => c.PutObjectAsync(It.IsAny<PutObjectRequest>(), cancellationToken))
            .Callback<PutObjectRequest, CancellationToken>((request, _) =>
            {
                capturedRequest = request;
                if (request.InputStream != null && request.InputStream.CanSeek)
                {
                    capturedLength = request.InputStream.Length;
                }
            })
            .ReturnsAsync(new PutObjectResponse { HttpStatusCode = System.Net.HttpStatusCode.OK });

        string expectedTempPath = Path.Combine(Path.GetTempPath(), attachmentName);

        // When: saving the attachment.
        Result<string> result = await Repository.SaveAttachment(templateId, attachmentStream, attachmentName, cancellationToken);

        // Then
        Assert.That(result.IsSuccess);
        Assert.That(File.Exists(expectedTempPath), Is.False);
        Assert.That(capturedRequest, Is.Not.Null);
        Assert.That(capturedRequest!.BucketName, Is.EqualTo(Options.EmailTemplatesBucketName));
        Assert.That(capturedRequest.Key, Is.EqualTo($"{templateId}/attachments/{attachmentName}"));
        Assert.That(capturedRequest.InputStream, Is.Not.Null);
        Assert.That(capturedLength, Is.EqualTo(expectedBytes.Length));

        ClientMock.Verify(
            c => c.PutObjectAsync(It.IsAny<PutObjectRequest>(), cancellationToken),
            Times.Once);
    }

    [Test]
    [Description("Given an upload failure, when saving the attachment, then it should delete the temp file and propagate the exception")]
    public void ShouldDeleteTempFileWhenUploadThrows()
    {
        // Given: an upload exception to verify temp cleanup on failure.
        string templateId = Guid.NewGuid().ToString("N");
        string attachmentName = $"{Guid.NewGuid():N}.bin";
        byte[] bytes = Encoding.UTF8.GetBytes("boom");
        using MemoryStream attachmentStream = new MemoryStream(bytes);

        CancellationToken cancellationToken = CancellationToken.None;

        ClientMock
            .Setup(c => c.PutObjectAsync(It.IsAny<PutObjectRequest>(), cancellationToken))
            .ThrowsAsync(new IOException("Upload failed"));

        string expectedTempPath = Path.Combine(Path.GetTempPath(), attachmentName);

        // When / Then: saving should throw and temp file should be deleted.
        Assert.That(
            async () => await Repository.SaveAttachment(templateId, attachmentStream, attachmentName, cancellationToken),
            Throws.TypeOf<IOException>());

        Assert.That(File.Exists(expectedTempPath), Is.False);
    }

    [Test]
    [Description("Given a stream copy failure, when saving the attachment, then it should delete the temp file and propagate the exception")]
    public void ShouldDeleteTempFileWhenCopyToThrows()
    {
        // Given: a stream that throws to verify temp cleanup on copy failures.
        string templateId = Guid.NewGuid().ToString("N");
        string attachmentName = $"{Guid.NewGuid():N}.bin";
        using ThrowingStream attachmentStream = new ThrowingStream();

        string expectedTempPath = Path.Combine(Path.GetTempPath(), attachmentName);

        // When / Then: saving should throw and temp file should be deleted.
        Assert.That(
            async () => await Repository.SaveAttachment(templateId, attachmentStream, attachmentName, CancellationToken.None),
            Throws.TypeOf<IOException>());

        Assert.That(File.Exists(expectedTempPath), Is.False);
    }

    private sealed class ThrowingStream : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => 0;
        public override long Position
        {
            get => 0;
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new IOException("Read failed");
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
