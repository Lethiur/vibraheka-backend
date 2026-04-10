using CSharpFunctionalExtensions;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.EmailTemplates.Queries.GetAllEmailTemplates;
using VibraHeka.Domain.EmailTemplates.Ports.Out;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.FunctionalTests.EmailTemplates.Queries.GetAllEmailTemplatesTest;

[TestFixture]
public class GetAllEmailTemplatesQueryHandlerTest
{
    private Mock<EmailTemplatePort> EmailTemplatesServiceMock;
    private GetAllEmailTemplatesQueryHandler Handler;

    [SetUp]
    public void SetUp()
    {
        EmailTemplatesServiceMock = new Mock<EmailTemplatePort>();

        Handler = new GetAllEmailTemplatesQueryHandler(
            EmailTemplatesServiceMock.Object);
    }

 
    [Test]
    [Description("Given a request for all templates, when the service returns a successful list, then the handler should return that list")]
    public async Task ShouldReturnTemplatesIfEverythingIsOk()
    {
        // Given
        IEnumerable<EmailTemplateEntity> templates = new List<EmailTemplateEntity> { new() { TemplateID = "1", Name = "Welcome" } };

        EmailTemplatesServiceMock.Setup(x => x.GetAllTemplates(CancellationToken.None))
            .ReturnsAsync(Result.Success(templates));
        GetAllEmailTemplatesQuery query = new();

        // When
        Result<IEnumerable<EmailTemplateEntity>> result = await Handler.Handle(query, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(templates));
        EmailTemplatesServiceMock.Verify(x => x.GetAllTemplates(CancellationToken.None), Times.Once);
    }

    [Test]
    [Description("Given a request for all templates, when the service returns a failure, then the handler should return that failure")]
    public async Task ShouldReturnFailureIfServiceFails()
    {
        // Given
        const string errorMessage = "Error fetching from DynamoDB";

        EmailTemplatesServiceMock.Setup(x => x.GetAllTemplates(CancellationToken.None))
            .ReturnsAsync(Result.Failure<IEnumerable<EmailTemplateEntity>>(errorMessage));
        GetAllEmailTemplatesQuery query = new();

        // When
        Result<IEnumerable<EmailTemplateEntity>> result = await Handler.Handle(query, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(errorMessage));
    }
}
