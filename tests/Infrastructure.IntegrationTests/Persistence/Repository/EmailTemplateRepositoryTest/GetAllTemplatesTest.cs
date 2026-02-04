using System.ComponentModel;
using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.EmailTemplateRepositoryTest;

[TestFixture]
public class GetAllTemplatesTest : GenericEmailTemplateRepositoryIntegrationTest
{
    [Test]
    [DisplayName("Should return all templates including seeded ones")]
    public async Task ShouldReturnAllTemplatesIncludingSeededOnes()
    {
        // Given
        string templateId1 = $"it-{Guid.NewGuid():N}";
        string templateId2 = $"it-{Guid.NewGuid():N}";

        SaveConfig saveConfig = new() { OverrideTableName = _configuration.EmailTemplatesTable };
        await DynamoContext.SaveAsync(new EmailTemplateDBModel
        {
            TemplateID = templateId1,
            Name = "Name 1",
            Path = "Path 1",
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        }, saveConfig);
        await DynamoContext.SaveAsync(new EmailTemplateDBModel
        {
            TemplateID = templateId2,
            Name = "Name 2",
            Path = "Path 2",
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        }, saveConfig);

        // When
        Result<IEnumerable<EmailEntity>> result = await Repository.GetAllTemplates(CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Has.Some.Matches<EmailEntity>(t => t.ID == templateId1));
        Assert.That(result.Value, Has.Some.Matches<EmailEntity>(t => t.ID == templateId2));
    }
}

