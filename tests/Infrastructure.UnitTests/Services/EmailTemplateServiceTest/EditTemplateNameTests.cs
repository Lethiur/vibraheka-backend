using System.ComponentModel;
using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.UnitTests.Services.EmailTemplateServiceTest;

[TestFixture]
public class EditTemplateNameTests
{
    private Mock<IEmailTemplatesRepository> _repositoryMock;
    private EmailTemplateService _service;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<IEmailTemplatesRepository>(MockBehavior.Strict);
        _service = new EmailTemplateService(_repositoryMock.Object);
    }

    [Test]
    [DisplayName("Should update template name and call repository save when template exists")]
    public async Task ShouldUpdateTemplateNameAndCallRepositorySaveWhenTemplateExists()
    {
        // Given
        const string templateId = "template-1";
        const string newName = "New Name";
        DateTimeOffset initialLastModified = DateTimeOffset.UtcNow.AddDays(-1);

        EmailEntity template = new()
        {
            ID = templateId,
            Name = "Old Name",
            Path = "path",
            LastModified = initialLastModified
        };

        EmailEntity? savedEntity = null;

        _repositoryMock
            .Setup(x => x.GetTemplateByID(templateId))
            .ReturnsAsync(Result.Success(template));

        _repositoryMock
            .Setup(x => x.SaveTemplate(It.IsAny<EmailEntity>(), It.IsAny<CancellationToken>()))
            .Callback<EmailEntity, CancellationToken>((entity, _) => savedEntity = entity)
            .ReturnsAsync(Result.Success(Unit.Value));

        // When
        Result<Unit> result = await _service.EditTemplateName(templateId, newName, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(savedEntity, Is.Not.Null);
        Assert.That(savedEntity!.ID, Is.EqualTo(templateId));
        Assert.That(savedEntity.Name, Is.EqualTo(newName));
        Assert.That(savedEntity.LastModified, Is.GreaterThan(initialLastModified));

        _repositoryMock.Verify(x => x.GetTemplateByID(templateId), Times.Once);
        _repositoryMock.Verify(x => x.SaveTemplate(It.IsAny<EmailEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    [DisplayName("Should return InvalidTempalteID and not call repository when template id is invalid")]
    [TestCase(null!)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task ShouldReturnInvalidTemplateIdAndNotCallRepositoryWhenTemplateIdIsInvalid(string invalidTemplateId)
    {
        Result<Unit> result = await _service.EditTemplateName(invalidTemplateId, "New Name", CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(EmailTemplateErrors.InvalidTempalteID));

        _repositoryMock.Verify(x => x.GetTemplateByID(It.IsAny<string>()), Times.Never);
        _repositoryMock.Verify(x => x.SaveTemplate(It.IsAny<EmailEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    [DisplayName("Should return TemplateNotFound when repository returns null template")]
    public async Task ShouldReturnTemplateNotFoundWhenRepositoryReturnsNullTemplate()
    {
        // Given
        const string templateId = "missing-template";

        _repositoryMock
            .Setup(x => x.GetTemplateByID(templateId))
            .ReturnsAsync(Result.Success<EmailEntity>(null!));

        // When
        Result<Unit> result = await _service.EditTemplateName(templateId, "New Name", CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(EmailTemplateErrors.TemplateNotFound));

        _repositoryMock.Verify(x => x.SaveTemplate(It.IsAny<EmailEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    [DisplayName("Should propagate repository failure and not call save")]
    public async Task ShouldPropagateRepositoryFailureAndNotCallSave()
    {
        // Given
        const string templateId = "template-err";
        const string repositoryError = "some-error";

        _repositoryMock
            .Setup(x => x.GetTemplateByID(templateId))
            .ReturnsAsync(Result.Failure<EmailEntity>(repositoryError));

        // When
        Result<Unit> result = await _service.EditTemplateName(templateId, "New Name", CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(repositoryError));

        _repositoryMock.Verify(x => x.SaveTemplate(It.IsAny<EmailEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

