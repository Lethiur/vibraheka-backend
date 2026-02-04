using System.ComponentModel;
using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.UnitTests.Services.EmailTemplateServiceTest;

public class GetTemplateByIDTest
{
    private Mock<IEmailTemplatesRepository> _repositoryMock;
    private EmailTemplateService _service;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<IEmailTemplatesRepository>();
        _service = new EmailTemplateService(_repositoryMock.Object);
    }

    [Test]
    [DisplayName("Should return email template when a valid template ID is provided")]
    public async Task ShouldReturnEmailTemplateWhenValidIdProvided()
    {
        // Given: A valid template ID and a template in the repository
        const string templateId = "welcome-email";
        EmailEntity template = new EmailEntity { ID = templateId, Path = "Welcome!" };

        _repositoryMock.Setup(x => x.GetTemplateByID(templateId))
            .ReturnsAsync(Result.Success(template));

        // When: Getting the template by ID
        Result<EmailEntity> result = await _service.GetTemplateByID(templateId);

        // Then: Should return success with the template
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(template));
        _repositoryMock.Verify(x => x.GetTemplateByID(templateId), Times.Once);
    }

    [Test]
    [DisplayName("Should return InvalidTemplateID error when ID is null or whitespace")]
    [TestCase(null!)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task ShouldReturnInvalidTemplateIdWhenIdIsInvalid(string invalidId)
    {
        // Given: An invalid template ID

        // When: Getting the template
        Result<EmailEntity> result = await _service.GetTemplateByID(invalidId);

        // Then: Should fail with InvalidTemplateID error without calling the repository
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(EmailTemplateErrors.InvalidTempalteID));
        _repositoryMock.Verify(x => x.GetTemplateByID(It.IsAny<string>()), Times.Never);
    }

    [Test]
    [DisplayName("Should return repository error when template is not found")]
    public async Task ShouldReturnRepositoryErrorWhenTemplateNotFound()
    {
        // Given: A valid ID but the repository cannot find the template
        const string templateId = "non-existent";
        const string repoError = "Template not found in DB";

        _repositoryMock.Setup(x => x.GetTemplateByID(templateId))
            .ReturnsAsync(Result.Failure<EmailEntity>(repoError));

        // When: Getting the template
        Result<EmailEntity> result = await _service.GetTemplateByID(templateId);

        // Then: Should propagate the failure from the repository
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(repoError));
    }
}
