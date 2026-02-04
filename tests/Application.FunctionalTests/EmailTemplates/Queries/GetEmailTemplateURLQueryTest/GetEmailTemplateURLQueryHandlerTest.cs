using CSharpFunctionalExtensions;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.EmailTemplates.Queries.GetEmailTemplateURL;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;

namespace VibraHeka.Application.FunctionalTests.EmailTemplates.Queries.GetEmailTemplateURLQueryTest;

[TestFixture]
public class GetEmailTemplateURLQueryHandlerTest
{
    private Mock<IEmailTemplateStorageService> _storageServiceMock = default!;
    private GetEmailTemplateURLQueryHandler _handler = default!;

    [SetUp]
    public void SetUp()
    {
        _storageServiceMock = new Mock<IEmailTemplateStorageService>();
        _handler = new GetEmailTemplateURLQueryHandler(_storageServiceMock.Object);
    }

    [Test]
    [Description("Given a valid TemplateID, when the storage service returns a success result, then the handler should return the URL")]
    public async Task ShouldReturnUrlWhenStorageSucceeds()
    {
        // Given
        GetEmailTemplateURLQuery query = new("template-1");
        string expectedUrl = "https://example.com/url";
        _storageServiceMock
            .Setup(x => x.GetTemplateUrlAsync(query.TemplateID, CancellationToken.None))
            .ReturnsAsync(Result.Success(expectedUrl));

        // When
        Result<string> result = await _handler.Handle(query, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedUrl));
        _storageServiceMock.Verify(x => x.GetTemplateUrlAsync(query.TemplateID, CancellationToken.None), Times.Once);
    }

    [Test]
    [Description("Given a valid TemplateID, when the storage service returns a failure result, then the handler should return the same failure")]
    public async Task ShouldReturnFailureWhenStorageFails()
    {
        // Given
        GetEmailTemplateURLQuery query = new("template-1");
        string errorMessage = "ET-002";
        _storageServiceMock
            .Setup(x => x.GetTemplateUrlAsync(query.TemplateID, CancellationToken.None))
            .ReturnsAsync(Result.Failure<string>(errorMessage));

        // When
        Result<string> result = await _handler.Handle(query, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(errorMessage));
    }
}
