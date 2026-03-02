using Amazon.DynamoDBv2.DataModel;
using Moq;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Mappers;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.VerificationCodesRepositoryTest;

public abstract class GenericVerificationCodesRepositoryTest
{
    protected Mock<IDynamoDBContext> ContextMock;
    protected AWSConfig ConfigMock;
    protected VerificationCodesRepository Repository;

    [SetUp]
    public void SetUp()
    {
        ContextMock = new Mock<IDynamoDBContext>();
        ConfigMock = new AWSConfig();
#if DEBUG
        ConfigMock.CodesTable = "CodesTable";
#endif
        Repository = new VerificationCodesRepository(ContextMock.Object, ConfigMock, new VerificationCodeEntityMapper());
    }
}
