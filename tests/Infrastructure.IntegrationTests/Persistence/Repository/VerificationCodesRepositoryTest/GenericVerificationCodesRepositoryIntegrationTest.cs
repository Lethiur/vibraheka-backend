using Amazon.DynamoDBv2.DataModel;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.VerificationCodesRepositoryTest;

public abstract class GenericVerificationCodesRepositoryIntegrationTest : TestBase
{
    protected IDynamoDBContext _dynamoDbContext;
    protected ICodeRepository _repository;

    [OneTimeSetUp]
    public void OneTimeSetUpChild()
    {
        base.OneTimeSetUp();
        _dynamoDbContext = CreateDynamoDBContext();
        _repository = new VerificationCodesRepository(_dynamoDbContext, _configuration, new VerificationCodeEntityMapper());
    }

    [OneTimeTearDown]
    public void OneTimeTearDownChild()
    {
        _dynamoDbContext.Dispose();
    }
}
