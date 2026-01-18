using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;
using VibraHeka.Infrastructure.Persistence.Repository;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.EmailTemplateServiceTest;

[TestFixture]
public class GetAllTemplateTest : TestBase
{
    private IDynamoDBContext _context;
    private EmailTemplateRepository _repository;
    private EmailTemplateService _service;
    private string _tableName;

    [SetUp]
    public void SetUp()
    {
        // Given
        _context = CreateDynamoDBContext();
        _tableName = _configuration["Dynamo:EmailTemplatesTable"]!;

        // Inicializamos el repositorio con la configuración y el contexto de la base
        _repository = new EmailTemplateRepository(_context, _configuration);
        _service = new EmailTemplateService(_repository);
    }

    [TearDown]
    public void TearDown()
    {
        _context?.Dispose();
    }

    [Test]
    public async Task ShouldGetAllTemplatesFromDynamoDB()
    {
        // Given
        EmailTemplateDBModel template1 = new EmailTemplateDBModel
        {
            TemplateID = _faker.Random.Guid().ToString(), Name = _faker.Commerce.ProductName(),
        };
        EmailTemplateDBModel template2 = new EmailTemplateDBModel()
        {
            TemplateID = _faker.Random.Guid().ToString(), Name = _faker.Commerce.ProductName(),
        };

        // Guardamos las plantillas usando el contexto real en la tabla de test
        SaveConfig config = new SaveConfig() { OverrideTableName = _tableName };
        await _context.SaveAsync(template1, config, CancellationToken.None);
        await _context.SaveAsync(template2, config, CancellationToken.None);

        // When
        Result<IEnumerable<EmailEntity>> result = await _service.GetAllTemplates(CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Has.Some.Matches<EmailEntity>(x => x.ID == template1.TemplateID));
        Assert.That(result.Value, Has.Some.Matches<EmailEntity>(x => x.ID == template2.TemplateID));
    }
}
