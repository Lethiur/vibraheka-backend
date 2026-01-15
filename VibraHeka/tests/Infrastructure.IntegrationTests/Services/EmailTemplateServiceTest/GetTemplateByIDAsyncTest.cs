using System.ComponentModel;
using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;
using VibraHeka.Infrastructure.Persistence.Repository;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.EmailTemplateServiceTest;

[TestFixture]
public class GetTemplateByIDAsyncTest : TestBase
{
    private EmailTemplateService _service;
    private EmailTemplateRepository _repository;
    private IDynamoDBContext _context;

    [SetUp]
    public void SetUp()
    {
        _context = CreateDynamoDBContext();
        _repository = new EmailTemplateRepository(_context, _configuration);
        _service = new EmailTemplateService(_repository);
    }

    [TearDown]
    public void TearDown()
    {
        _context?.Dispose();
    }

    [Test]
    [DisplayName("Should retrieve email template from DynamoDB when valid ID is provided")]
    public async Task ShouldRetrieveEmailTemplateFromDynamoDBWhenValidIdProvided()
    {
        // Given: A template persisted in the DynamoDB table
        var templateId = $"test-template-{Guid.NewGuid()}";
        var expectedTemplate = new EmailTemplateDBModel()
        {
            TemplateID = templateId, Path = "Integration Test Subject"
        };

        await _context.SaveAsync(expectedTemplate,
            new SaveConfig() { OverrideTableName = _configuration["Dynamo:EmailTemplatesTable"] });

        // When: Retrieving the template through the service
        Result<EmailEntity> result = await _service.GetTemplateByID(templateId);

        // Then: The operation should be successful and match the saved data
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.ID, Is.EqualTo(templateId));
        Assert.That(result.Value.Path, Is.EqualTo(expectedTemplate.Path));
    }

    [Test]
    [DisplayName("Should return failure when template ID does not exist in DynamoDB")]
    public async Task ShouldReturnFailureWhenTemplateIdDoesNotExist()
    {
        // Given: An ID that is not in the table
        string nonExistentId = "non-existent-id-123";

        // When: Retrieving the template
        Result<EmailEntity> result = await _service.GetTemplateByID(nonExistentId);

        // Then: Should return success with null or failure depending on repository implementation
        // Basándonos en GenericDynamoRepository, si LoadAsync devuelve null, suele devolverse success(null)
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(EmailTemplateErrors.TemplateNotFound));
    }

    [Test]
    [DisplayName("Should return InvalidTemplateID error without calling DB when ID is whitespace")]
    public async Task ShouldReturnInvalidTemplateIdErrorWhenIdIsWhitespace()
    {
        // Given: An invalid ID string
        const string invalidId = "   ";

        // When: Retrieving the template
        Result<EmailEntity> result = await _service.GetTemplateByID(invalidId);

        // Then: The service validation should catch it before repository
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(EmailTemplateErrors.InvalidTempalteID));
    }
}
