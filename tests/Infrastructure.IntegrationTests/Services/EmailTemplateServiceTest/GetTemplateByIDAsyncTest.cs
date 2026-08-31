using System.ComponentModel;
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.EmailTemplateServiceTest;

[TestFixture]
public class GetTemplateByIDAsyncTest : GenericEmailTemplateServiceTest
{


    [Test]
    [DisplayName("Should retrieve email template from DynamoDB when valid ID is provided")]
    public async Task ShouldRetrieveEmailTemplateFromDynamoDbWhenValidIdProvided()
    {
        // Given: A template persisted in the DynamoDB table
        Guid templateId = Guid.NewGuid();
        EmailTemplateDBModel expectedTemplate = new()
        {
            TemplateID = templateId.ToString(),
            Path = "Integration Test Subject"
        };

        await _context.SaveAsync(expectedTemplate);

        // When: Retrieving the template through the service
        Result<EmailEntity> result = await _service.GetTemplateByID(templateId, CancellationToken.None);

        // Then: The operation should be successful and match the saved data
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.ID, Is.EqualTo(templateId.ToString()));
        Assert.That(result.Value.Path, Is.EqualTo(expectedTemplate.Path));
    }

    [Test]
    [DisplayName("Should return failure when template ID does not exist in DynamoDB")]
    public async Task ShouldReturnFailureWhenTemplateIdDoesNotExist()
    {
        
        // When: Retrieving the template
        Result<EmailEntity> result = await _service.GetTemplateByID(Guid.NewGuid(), CancellationToken.None);

        // Then: Should return success with null or failure depending on repository implementation
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(EmailTemplateErrors.TemplateNotFound));
    }

    [Test]
    [DisplayName("Should return InvalidTemplateID error without calling DB when ID is whitespace")]
    public async Task ShouldReturnInvalidTemplateIdErrorWhenIdIsWhitespace()
    {

        // When: Retrieving the template
        Result<EmailEntity> result = await _service.GetTemplateByID(Guid.Empty, CancellationToken.None);

        // Then: The service validation should catch it before repository
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(EmailTemplateErrors.InvalidTempalteID));
    }
}
