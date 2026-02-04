using System.ComponentModel;
using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;
using static System.Threading.CancellationToken;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.EmailTemplateRepositoryTest;

[TestFixture]
public class GetTemplateByIDAsync : GenericEmailTemplateRepositoryTest
{
    [Test]
    [DisplayName("Should return email template when it exists in DynamoDB")]
    public async Task ShouldReturnEmailTemplateWhenExists()
    {
        // Given: A valid template ID and a template in DynamoDB
        const string templateId = "welcome-template";
        EmailTemplateDBModel template = new EmailTemplateDBModel { TemplateID = templateId, Path = "Welcome" };

        _contextMock.Setup(x => x.LoadAsync<EmailTemplateDBModel>(templateId, It.IsAny<LoadConfig>(), None))
            .ReturnsAsync(template);

        // When: Retrieving the template
        Result<EmailEntity> result = await Repository.GetTemplateByID(templateId);

        // Then: Should return success with the template
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.ID, Is.EqualTo(template.TemplateID));
        _contextMock.Verify(x => x.LoadAsync<EmailTemplateDBModel>(templateId, 
            It.Is<LoadConfig>(c => c.OverrideTableName == TableName), None), Times.Once);
    }

    [Test]
    [DisplayName("Should return successful result with null when template does not exist")]
    public async Task ShouldFailureWithNullWhenNotFound()
    {
        // Given: An ID that is not in DynamoDB
        const string templateId = "missing-template";
        _contextMock.Setup(x => x.LoadAsync<EmailTemplateDBModel>(templateId, It.IsAny<LoadConfig>(), None))
            .ReturnsAsync((EmailTemplateDBModel)null!);

        // When: Retrieving the template
        Result<EmailEntity> result = await Repository.GetTemplateByID(templateId);

        // Then: Should return success but the value should be null (comportamiento de LoadAsync)
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(EmailTemplateErrors.TemplateNotFound));
    }

    [Test]
    [DisplayName("Should return failure when DynamoDB throws an exception")]
    public async Task ShouldReturnFailureWhenExceptionOccurs()
    {
        // Given: A database error
        const string templateId = "any-id";
        _contextMock.Setup(x => x.LoadAsync<EmailTemplateDBModel>(templateId, It.IsAny<LoadConfig>(), None))
            .ThrowsAsync(new Exception("DynamoDB error"));

        // When: Retrieving the template
        Result<EmailEntity> result = await Repository.GetTemplateByID(templateId);

        // Then: Should fail with the handled error message
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Does.Contain("DynamoDB error"));
    }
}
