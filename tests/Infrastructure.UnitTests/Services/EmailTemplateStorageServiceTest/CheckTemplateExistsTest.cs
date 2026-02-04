using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Infrastructure.UnitTests.Services.EmailTemplateStorageServiceTest;

[TestFixture]
[Category("unit")]
public class CheckTemplateExistsTest : GenericEmailTemplateStorageServiceTest
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task ShouldReturnInvalidTemplateIdWhenTemplateIdIsInvalid(string? templateId)
    {
        Result<Unit> result = await Service.CheckTemplateExists(templateId!, TestCancellationToken);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(EmailTemplateErrors.InvalidTempalteID));
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
        Result<Unit> result = await Service.CheckTemplateExists(templateId, TestCancellationToken);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(EmailTemplateErrors.TemplateNotFound));
        RepositoryMock.Verify(r => r.TemplateExistsAsync(templateId, TestCancellationToken), Times.Once);
        RepositoryMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ShouldReturnSuccessWhenTemplateExists()
    {
        // Given
        const string templateId = "existing-template";

        RepositoryMock
            .Setup(r => r.TemplateExistsAsync(templateId, TestCancellationToken))
            .ReturnsAsync(Result.Success(true));

        // When
        Result<Unit> result = await Service.CheckTemplateExists(templateId, TestCancellationToken);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        RepositoryMock.Verify(r => r.TemplateExistsAsync(templateId, TestCancellationToken), Times.Once);
        RepositoryMock.VerifyNoOtherCalls();
    }
}
