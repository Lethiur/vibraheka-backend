using System.ComponentModel;
using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;
using VibraHeka.Infrastructure.Persistence.Repository;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.EmailTemplateServiceTest;

[TestFixture]
public class EditTemplateNameTests : TestBase
{
    private IDynamoDBContext _context;
    private EmailTemplateRepository _repository;
    private EmailTemplateService _service;

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
    [DisplayName("Should update template name and last modified when template exists")]
    public async Task ShouldUpdateTemplateNameAndLastModifiedWhenTemplateExists()
    {
        // Given
        string templateId = $"test-template-{Guid.NewGuid()}";
        DateTimeOffset initialLastModified = DateTimeOffset.UtcNow.AddDays(-1);
        string initialName = $"initial-{Guid.NewGuid()}";
        string newName = $"renamed-{Guid.NewGuid()}";

        EmailTemplateDBModel persisted = new()
        {
            TemplateID = templateId,
            Path = "Integration Test Path",
            Name = initialName,
            Created = DateTimeOffset.UtcNow.AddDays(-2),
            LastModified = initialLastModified
        };

        await _context.SaveAsync(persisted, new SaveConfig { OverrideTableName = _configuration.EmailTemplatesTable });

        // When
        Result<Unit> editResult = await _service.EditTemplateName(templateId, newName, CancellationToken.None);

        // Then
        Assert.That(editResult.IsSuccess, Is.True);

        EmailEntity updated = await WaitForUpdatedTemplate(templateId, newName, timeout: TimeSpan.FromSeconds(10));
        Assert.That(updated.Name, Is.EqualTo(newName));
        Assert.That(updated.LastModified, Is.GreaterThan(initialLastModified));
    }

    [Test]
    [DisplayName("Should return InvalidTempalteID when template id is whitespace")]
    public async Task ShouldReturnInvalidTemplateIdWhenTemplateIdIsWhitespace()
    {
        Result<Unit> result = await _service.EditTemplateName("   ", "any-name", CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(EmailTemplateErrors.InvalidTempalteID));
    }

    [Test]
    [DisplayName("Should return TemplateNotFound when template does not exist")]
    public async Task ShouldReturnTemplateNotFoundWhenTemplateDoesNotExist()
    {
        string nonExistentId = $"non-existent-{Guid.NewGuid()}";

        Result<Unit> result = await _service.EditTemplateName(nonExistentId, "new-name", CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(EmailTemplateErrors.TemplateNotFound));
    }

    private async Task<EmailEntity> WaitForUpdatedTemplate(string templateId, string expectedName, TimeSpan timeout)
    {
        DateTimeOffset start = DateTimeOffset.UtcNow;
        while (DateTimeOffset.UtcNow - start < timeout)
        {
            Result<EmailEntity> result = await _service.GetTemplateByID(templateId);
            if (result.IsSuccess && result.Value.Name == expectedName) return result.Value;
            await Task.Delay(250);
        }

        Assert.Fail($"Template '{templateId}' was not updated to '{expectedName}' within {timeout.TotalSeconds} seconds.");
        throw new InvalidOperationException("Unreachable");
    }
}

