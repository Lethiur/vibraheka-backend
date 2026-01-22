using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.UnitTests.Services.EmailTemplateServiceTest;

public class GetAllTemplatesTest
{
    private Mock<IEmailTemplatesRepository> EmailTemplateRepositoryMock;
    private EmailTemplateService Service;

    [SetUp]
    public void SetUp()
    {
        EmailTemplateRepositoryMock = new Mock<IEmailTemplatesRepository>();
        Service = new EmailTemplateService(EmailTemplateRepositoryMock.Object);
    }

    [Test]
    public async Task ShouldReturnAllTemplatesFromRepository()
    {
        // Given
        var expectedTemplates = new List<EmailEntity>
        {
            new EmailEntity { ID = "1", Name = "Template 1" },
            new EmailEntity { ID = "2", Name = "Template 2" }
        };

        EmailTemplateRepositoryMock.Setup(x => x.GetAllTemplates(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IEnumerable<EmailEntity>>(expectedTemplates));

        // When
        Result<IEnumerable<EmailEntity>> result = await Service.GetAllTemplates(CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedTemplates));
        EmailTemplateRepositoryMock.Verify(x => x.GetAllTemplates(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ShouldReturnFailureIfRepositoryFails()
    {
        // Given
        const string errorMessage = "Repository error";
        EmailTemplateRepositoryMock.Setup(x => x.GetAllTemplates(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<IEnumerable<EmailEntity>>(errorMessage));

        // When
        Result<IEnumerable<EmailEntity>> result = await Service.GetAllTemplates(CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(errorMessage));
    }
}
