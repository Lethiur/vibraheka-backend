using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.EmailTemplateServiceTest;

[TestFixture]
public class GetAllTemplateTest : GenericEmailTemplateServiceTest
{
    
    [Test]
    public async Task ShouldGetAllTemplatesFromDynamoDB()
    {
        // Given
        EmailTemplateDBModel template1 = new()
        {
            TemplateID = _faker.Random.Guid().ToString(), Name = _faker.Commerce.ProductName(),
        };
        EmailTemplateDBModel template2 = new()
        {
            TemplateID = _faker.Random.Guid().ToString(), Name = _faker.Commerce.ProductName(),
        };

        // Guardamos las plantillas usando el contexto real en la tabla de test
        SaveConfig config = new() { OverrideTableName = _configuration.EmailTemplatesTable };
        await _context.SaveAsync(template1, config, CancellationToken.None);
        await _context.SaveAsync(template2, config, CancellationToken.None);

        // When
        Result<IEnumerable<EmailTemplateEntity>> result = await _service.GetAllTemplates(CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Has.Some.Matches<EmailTemplateEntity>(x => x.TemplateID == template1.TemplateID));
        Assert.That(result.Value, Has.Some.Matches<EmailTemplateEntity>(x => x.TemplateID == template2.TemplateID));
    }
}
