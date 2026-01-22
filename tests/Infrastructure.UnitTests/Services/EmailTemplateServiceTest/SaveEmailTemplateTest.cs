using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.UnitTests.Services.EmailTemplateServiceTest;

[TestFixture]
public class SaveEmailTemplateTest
{
    private Mock<IEmailTemplatesRepository> EmailTemplateRepositoryMock;
    private EmailTemplateService Service;

    [SetUp]
    public void SetUp()
    {
        EmailTemplateRepositoryMock = new Mock<IEmailTemplatesRepository>(MockBehavior.Strict);

        Service = new EmailTemplateService(
            EmailTemplateRepositoryMock.Object
        );
    }

    [Test]
    [Description(
        "Given a null email template entity, when saving the template, then it should return InvalidTemplateEntity error and not call the repository")]
    public async Task ShouldReturnFailureWhenEmailTemplateIsNull()
    {
        // Given
        EmailEntity emailTemplate = null!;
        CancellationToken token = CancellationToken.None;

        // When
        Result<string> result = await Service.SaveEmailTemplate(emailTemplate, token);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(EmailTemplateErrors.InvalidTemplateEntity));

        EmailTemplateRepositoryMock.Verify(
            x => x.SaveTemplate(It.IsAny<EmailEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    [Description(
        "Given a valid email template entity, when saving the template, then it should call the repository and return the template ID")]
    public async Task ShouldSaveTemplateAndReturnIdWhenEmailTemplateIsValid()
    {
        // Given
        EmailEntity emailTemplate = new EmailEntity
        {
            ID = "template-id-123", Name = "Welcome", Path = "template-id-123"
        };
        CancellationToken token = CancellationToken.None;

        EmailTemplateRepositoryMock
            .Setup(x => x.SaveTemplate(emailTemplate, token))
            .ReturnsAsync(Result.Success(Unit.Value));

        // When
        Result<string> result = await Service.SaveEmailTemplate(emailTemplate, token);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("template-id-123"));

        EmailTemplateRepositoryMock.Verify(
            x => x.SaveTemplate(emailTemplate, token),
            Times.Once
        );
    }

    [Test]
    [Description(
        "Given a valid email template entity and the repository throws, when saving the template, then it should return failure")]
    public async Task ShouldReturnFailureWhenRepositoryThrows()
    {
        // Given
        EmailEntity emailTemplate = new EmailEntity
        {
            ID = "template-id-123", Name = "Welcome", Path = "template-id-123"
        };
        CancellationToken token = CancellationToken.None;

        EmailTemplateRepositoryMock
            .Setup(x => x.SaveTemplate(emailTemplate, token))
            .ThrowsAsync(new InvalidOperationException("Repository failure"));

        // When
        Result<string> result = await Service.SaveEmailTemplate(emailTemplate, token);

        // Then
        Assert.That(result.IsFailure, Is.True);

        EmailTemplateRepositoryMock.Verify(
            x => x.SaveTemplate(emailTemplate, token),
            Times.Once
        );
    }
}
