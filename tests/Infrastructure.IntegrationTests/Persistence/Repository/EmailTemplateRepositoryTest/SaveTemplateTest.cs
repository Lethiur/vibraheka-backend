using System.ComponentModel;
using Amazon.DynamoDBv2.DataModel;
using Bogus;
using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.EmailTemplateRepositoryTest;

[TestFixture]
public class SaveTemplateTest : TestBase
{
    private IEmailTemplatesRepository _repository;
    private IDynamoDBContext _dynamoContext;

    [OneTimeSetUp]
    public void OneTimeSetUpChild()
    {
        base.OneTimeSetUp();
        _dynamoContext = CreateDynamoDBContext();
        _repository = new Infrastructure.Persistence.Repository.EmailTemplateRepository(_dynamoContext, _configuration);
        _faker = new Faker();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _dynamoContext?.Dispose();
    }

    #region SaveTemplate - Success Cases

    [Test]
    [DisplayName("Should save email template successfully with valid ID and S3 path")]
    public async Task ShouldSaveTemplateSuccessfullyWhenValidDataProvided()
    {
        // Given: A valid email entity with ID and Path
        EmailEntity template = CreateValidTemplate();

        // When: Saving the template
        Result<Unit> result = await _repository.SaveTemplate(template, CancellationToken.None);

        // Then: Should return success
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    [DisplayName("Should save template successfully with deep S3 path structure")]
    public async Task ShouldSaveTemplateSuccessfullyWhenDeepPathProvided()
    {
        // Given: A template with a deep folder structure in S3
        EmailEntity template = new EmailEntity 
        { 
            ID = Guid.NewGuid().ToString(), 
            Path = "s3://my-bucket/templates/marketing/v1/user-welcome.html" 
        };

        // When: Saving the template
        Result<Unit> result = await _repository.SaveTemplate(template, CancellationToken.None);

        // Then: Should return success
        Assert.That(result.IsSuccess, Is.True);
    }

    #endregion

    #region SaveTemplate - Data Persistence Verification

    [Test]
    [DisplayName("Should persist ID and Path correctly in DynamoDB")]
    public async Task ShouldPersistTemplateDataCorrectlyWhenSaved()
    {
        // Given: A template with specific ID and path
        EmailEntity originalTemplate = CreateValidTemplate();

        // When: Saving the template
        await _repository.SaveTemplate(originalTemplate, CancellationToken.None);

        // And: Retrieving directly from DynamoDB
        LoadConfig loadConfig = new() { OverrideTableName = _configuration.EmailTemplatesTable };
        EmailTemplateDBModel? retrieved = await _dynamoContext.LoadAsync<EmailTemplateDBModel>(originalTemplate.ID, loadConfig);

        // Then: Values should match
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved.TemplateID, Is.EqualTo(originalTemplate.ID));
        Assert.That(retrieved.Path, Is.EqualTo(originalTemplate.Path));
        Assert.That(retrieved.Created, Is.EqualTo(originalTemplate.Created));
        Assert.That(retrieved.CreatedBy, Is.EqualTo(originalTemplate.CreatedBy));
        Assert.That(retrieved.LastModified, Is.EqualTo(originalTemplate.LastModified));
        Assert.That(retrieved.LastModifiedBy, Is.EqualTo(originalTemplate.LastModifiedBy));
    }

    #endregion

    #region SaveTemplate - Overwrite Cases

    [Test]
    [DisplayName("Should update Path when same template ID is saved with different location")]
    public async Task ShouldOverwritePathWhenSameIdSavedTwice()
    {
        // Given: An initial record
        string templateId = Guid.NewGuid().ToString();
        EmailEntity template = new EmailEntity { ID = templateId, Path = "path/old.html" };
        await _repository.SaveTemplate(template, CancellationToken.None);

        // And: The same ID but a new S3 path
        template.Path = "path/new.html";

        // When: Saving again
        Result<Unit> result = await _repository.SaveTemplate(template, CancellationToken.None);

        // Then: The path should be updated in DynamoDB
        Assert.That(result.IsSuccess, Is.True);
        
        LoadConfig loadConfig = new() { OverrideTableName = _configuration.EmailTemplatesTable };
        EmailTemplateDBModel? retrieved = await _dynamoContext.LoadAsync<EmailTemplateDBModel>(templateId, loadConfig);
        
        Assert.That(retrieved.Path, Is.EqualTo("path/new.html"));
    }

    #endregion

    #region Helper Methods

    private EmailEntity CreateValidTemplate()
    {
        return new EmailEntity
        {
            ID = Guid.NewGuid().ToString(),
            Path = $"templates/{_faker.System.FileName("html")}",
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow,
            CreatedBy = "Test User",
            LastModifiedBy = "Test User"
        };
    }


    #endregion
}
