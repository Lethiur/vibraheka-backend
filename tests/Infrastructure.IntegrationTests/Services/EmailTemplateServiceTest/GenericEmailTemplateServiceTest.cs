using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Logging;
using VibraHeka.Infrastructure.Persistence.Repository;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.EmailTemplateServiceTest;

public abstract class GenericEmailTemplateServiceTest : TestBase
{
    protected IDynamoDBContext _context;
    protected EmailTemplateRepository _repository;
    protected EmailTemplateService _service;

    [SetUp]
    public void SetUp()
    {
        _context = CreateDynamoDBContext();
        _repository = new EmailTemplateRepository(_context, _configuration, LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<EmailTemplateRepository>());
        _service = new EmailTemplateService(_repository);
    }

    [TearDown]
    public void TearDown()
    {
        _context?.Dispose();
    }
}
