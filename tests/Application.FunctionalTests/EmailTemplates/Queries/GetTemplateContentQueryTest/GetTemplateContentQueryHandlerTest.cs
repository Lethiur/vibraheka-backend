using CSharpFunctionalExtensions;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.EmailTemplates.Queries.GetTemplateContent;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.FunctionalTests.EmailTemplates.Queries.GetTemplateContentQueryTest;

[TestFixture]
public class GetTemplateContentQueryHandlerTest
{
    private Mock<IEmailTemplatesService> _templatesServiceMock = default!;
    private Mock<IEmailTemplateStorageService> _storageServiceMock = default!;
    private GetTemplateContentQueryHandler _handler = default!;

    [SetUp]
    public void SetUp()
    {
        _templatesServiceMock = new Mock<IEmailTemplatesService>();
        _storageServiceMock = new Mock<IEmailTemplateStorageService>();
        _handler = new GetTemplateContentQueryHandler(_templatesServiceMock.Object, _storageServiceMock.Object);
    }

    [Test]
    [Description("Given a valid TemplateID, when the template exists and storage returns content, then the handler should return the content")]
    public async Task ShouldReturnContentWhenTemplateExists()
    {
        // Given
        GetEmailTemplateContentQuery query = new("template-1");
        EmailEntity templateEntity = new() { ID = "template-1" };
        string expectedContent = "content";

        _templatesServiceMock
            .Setup(x => x.GetTemplateByID(query.TemplateID))
            .ReturnsAsync(Result.Success(templateEntity));

        _storageServiceMock
            .Setup(x => x.GetTemplateContent(templateEntity.ID, CancellationToken.None))
            .ReturnsAsync(Result.Success(expectedContent));

        // When
        Result<string> result = await _handler.Handle(query, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedContent));
    }

    [Test]
    [Description("Given a TemplateID, when the template service fails to find the template, then the handler should return a failure")]
    public async Task ShouldReturnFailureWhenTemplateServiceFails()
    {
        // Given
        GetEmailTemplateContentQuery query = new("template-1");
        string errorMessage = "ET-002";
        _templatesServiceMock
            .Setup(x => x.GetTemplateByID(query.TemplateID))
            .ReturnsAsync(Result.Failure<EmailEntity>(errorMessage));

        // When
        Result<string> result = await _handler.Handle(query, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(errorMessage));
        _storageServiceMock.Verify(x => x.GetTemplateContent(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    [Description("Given a valid template, when the storage service fails to retrieve content, then the handler should return a failure")]
    public async Task ShouldReturnFailureWhenStorageFails()
    {
        // Given
        GetEmailTemplateContentQuery query = new("template-1");
        EmailEntity templateEntity = new() { ID = "template-1" };
        string errorMessage = "S3-FAIL";

        _templatesServiceMock
            .Setup(x => x.GetTemplateByID(query.TemplateID))
            .ReturnsAsync(Result.Success(templateEntity));

        _storageServiceMock
            .Setup(x => x.GetTemplateContent(templateEntity.ID, CancellationToken.None))
            .ReturnsAsync(Result.Failure<string>(errorMessage));

        // When
        Result<string> result = await _handler.Handle(query, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(errorMessage));
    }
}
