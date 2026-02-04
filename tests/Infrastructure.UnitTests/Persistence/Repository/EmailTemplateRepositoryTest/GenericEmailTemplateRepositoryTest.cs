using VibraHeka.Infrastructure.Persistence.Repository;
using VibraHeka.Infrastructure.UnitTests.Persistence.Repository.DynamoRepositoryTest;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.EmailTemplateRepositoryTest;

public abstract class GenericEmailTemplateRepositoryTest : GenericDynamoRepositoryTest
{
    protected EmailTemplateRepository Repository = default!;

    [SetUp]
    public void SetUpEmailTemplateRepository()
    {
        base.SetUp();
        _configMock.EmailTemplatesTable = TableName;
        Repository = new EmailTemplateRepository(_contextMock.Object, _configMock);
    }
}

