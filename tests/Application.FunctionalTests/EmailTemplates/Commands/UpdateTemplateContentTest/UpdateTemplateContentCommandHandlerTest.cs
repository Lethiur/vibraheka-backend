using System.Text;
using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.EmailTemplates.Commands.UpdateTemplateContent;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.FunctionalTests.EmailTemplates.Commands;

[TestFixture]
public class UpdateTemplateContentCommandHandlerTest
{
    private Mock<IEmailTemplatesService> _templatesServiceMock = default!;
    private Mock<IEmailTemplateStorageService> _storageServiceMock = default!;
    private UpdateTemplateContentCommandHandler _handler = default!;

    [SetUp]
    public void SetUp()
    {
        _templatesServiceMock = new Mock<IEmailTemplatesService>();
        _storageServiceMock = new Mock<IEmailTemplateStorageService>();
        _handler = new UpdateTemplateContentCommandHandler(_templatesServiceMock.Object, _storageServiceMock.Object);
    }

    [Test]
    public async Task ShouldSaveTemplateContentWhenTemplateExists()
    {
        const string templateId = "template-123";
        EmailEntity templateEntity = new() { ID = templateId };
        using MemoryStream stream = new(Encoding.UTF8.GetBytes("new-content"));
        UpdateTemplateContentCommand command = new(templateId, stream);

        _templatesServiceMock
            .Setup(x => x.GetTemplateByID(templateId))
            .ReturnsAsync(Result.Success(templateEntity));

        _storageServiceMock
            .Setup(x => x.SaveTemplate(templateId, stream, CancellationToken.None))
            .ReturnsAsync(Result.Success("url"));

        Result<Unit> result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(Unit.Value));
        _storageServiceMock.Verify(x => x.SaveTemplate(templateId, stream, CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task ShouldReturnFailureWhenTemplateServiceFails()
    {
        const string templateId = "template-123";
        using MemoryStream stream = new(Encoding.UTF8.GetBytes("new-content"));
        UpdateTemplateContentCommand command = new(templateId, stream);

        _templatesServiceMock
            .Setup(x => x.GetTemplateByID(templateId))
            .ReturnsAsync(Result.Failure<EmailEntity>("ET-002"));

        Result<Unit> result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("ET-002"));
        _storageServiceMock.Verify(x => x.SaveTemplate(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ShouldReturnFailureWhenStorageSaveFails()
    {
        const string templateId = "template-123";
        EmailEntity templateEntity = new() { ID = templateId };
        using MemoryStream stream = new(Encoding.UTF8.GetBytes("new-content"));
        UpdateTemplateContentCommand command = new(templateId, stream);

        _templatesServiceMock
            .Setup(x => x.GetTemplateByID(templateId))
            .ReturnsAsync(Result.Success(templateEntity));

        _storageServiceMock
            .Setup(x => x.SaveTemplate(templateId, stream, CancellationToken.None))
            .ReturnsAsync(Result.Failure<string>("S3-FAIL"));

        Result<Unit> result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("S3-FAIL"));
    }
}

