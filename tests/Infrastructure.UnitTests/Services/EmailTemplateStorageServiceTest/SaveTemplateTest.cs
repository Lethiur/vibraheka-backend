using System.Text;
using Moq;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.UnitTests.Services.EmailTemplateStorageServiceTest;

[TestFixture]
[Category("unit")]
public class SaveTemplateTest
{
     private Mock<IEmailTemplateStorageRepository> _repositoryMock = default!;
    private EmailTemplateStorageService _service = default!;
    private CancellationToken _cancellationToken;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<IEmailTemplateStorageRepository>(MockBehavior.Strict);
        _service = new EmailTemplateStorageService(_repositoryMock.Object);
        _cancellationToken = CancellationToken.None;
    }

    [Test]
    public async Task ShouldCallRepositoryAndReturnTemplateIdWhenSaveTemplateIsCalled()
    {
        // Given
        string templateId = Guid.NewGuid().ToString("N");
        byte[] bytes = Encoding.UTF8.GetBytes("""{"template":"Hello"}""");

        using var templateStream = new MemoryStream(bytes);
        _repositoryMock
            .Setup(r => r.SaveTemplate(templateId, templateStream, _cancellationToken))
            .ReturnsAsync(CSharpFunctionalExtensions.Result.Success(MediatR.Unit.Value));

        // When
        var result = await _service.SaveTemplate(templateId, templateStream, _cancellationToken);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(templateId));

        _repositoryMock.Verify(r => r.SaveTemplate(templateId, templateStream, _cancellationToken), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Test]
    public void ShouldThrowWhenRepositoryThrowsWhenSaveTemplateIsCalled()
    {
        // Given
        string templateId = Guid.NewGuid().ToString("N");
        byte[] bytes = Encoding.UTF8.GetBytes("""{"template":"Boom"}""");

        using (var templateStream = new MemoryStream(bytes))
        {
            _repositoryMock
                .Setup(r => r.SaveTemplate(templateId, templateStream, _cancellationToken))
                .ThrowsAsync(new IOException("Upload failed"));

            // When / Then
            Assert.That(
                async () => await _service.SaveTemplate(templateId, templateStream, _cancellationToken),
                Throws.TypeOf<IOException>());

            _repositoryMock.Verify(r => r.SaveTemplate(templateId, templateStream, _cancellationToken), Times.Once);
            _repositoryMock.VerifyNoOtherCalls();
        }
    }
}
