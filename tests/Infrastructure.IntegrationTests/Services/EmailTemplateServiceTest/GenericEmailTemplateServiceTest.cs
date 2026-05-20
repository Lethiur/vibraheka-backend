using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using VibraHeka.Infrastructure.Persistence.Repository;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.EmailTemplateServiceTest;

public abstract class GenericEmailTemplateServiceTest : TestBase
{
    protected IDynamoDBContext _context;
    protected IAmazonDynamoDB _dynamoDbClient;
    protected EmailTemplateRepository _repository;
    protected EmailTemplateService _service;

    [SetUp]
    public void SetUp()
    {
        _context = CreateDynamoDBContext();
        _dynamoDbClient = CreateDynamoDBClient();
        _repository = new EmailTemplateRepository(_context, _dynamoDbClient, _configuration, CreateTestLogger<EmailTemplateRepository>());
        _service = new EmailTemplateService(_repository);
    }

    [TearDown]
    public void TearDown()
    {
        _context?.Dispose();
        _dynamoDbClient?.Dispose();
    }
}

