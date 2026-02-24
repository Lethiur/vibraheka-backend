using Microsoft.Extensions.Logging;
using Moq;
using VibraHeka.Infrastructure.Persistence.Repository;
using VibraHeka.Infrastructure.UnitTests.Persistence.Repository.DynamoRepositoryTest;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.EmailTemplateRepositoryTest;

public abstract class GenericEmailTemplateRepositoryTest : GenericDynamoRepositoryTest
{
    protected EmailTemplateRepository Repository = default!;
    protected Mock<ILogger<EmailTemplateRepository>> logger;
    
    [SetUp]
    public void SetUpEmailTemplateRepository()
    {
        base.SetUp();
        _configMock.EmailTemplatesTable = TableName;
        logger = new Mock<ILogger<EmailTemplateRepository>>();
        Repository = new EmailTemplateRepository(_contextMock.Object, _configMock, logger.Object);
    }
}

