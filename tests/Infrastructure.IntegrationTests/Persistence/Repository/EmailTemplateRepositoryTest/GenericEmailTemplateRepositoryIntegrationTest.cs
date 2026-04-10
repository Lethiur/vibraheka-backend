using Amazon.DynamoDBv2.DataModel;
using Bogus;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.EmailTemplateRepositoryTest;

public abstract class GenericEmailTemplateRepositoryIntegrationTest : TestBase
{
    protected IEmailTemplatesRepository Repository = default!;
    protected IDynamoDBContext DynamoContext = default!;

    [OneTimeSetUp]
    public void OneTimeSetUpEmailTemplateRepository()
    {
        base.OneTimeSetUp();
        DynamoContext = CreateDynamoDBContext();
        Repository = new EmailTemplateRepository(DynamoContext, _configuration, CreateTestLogger<EmailTemplateRepository>());
        _faker = new Faker();
    }

    [OneTimeTearDown]
    public void OneTimeTearDownEmailTemplateRepository()
    {
        DynamoContext?.Dispose();
    }
}


