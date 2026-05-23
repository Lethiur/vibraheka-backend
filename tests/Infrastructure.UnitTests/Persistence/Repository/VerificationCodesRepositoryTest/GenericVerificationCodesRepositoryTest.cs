using Amazon.DynamoDBv2.DataModel;
using Moq;
using VibraHeka.Infrastructure.Mappers;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.VerificationCodesRepositoryTest;

public abstract class GenericVerificationCodesRepositoryTest
{
    protected Mock<IDynamoDBContext> ContextMock;
    protected VerificationCodesRepository Repository;

    [SetUp]
    public void SetUp()
    {
        ContextMock = new Mock<IDynamoDBContext>();
        Repository = new VerificationCodesRepository(ContextMock.Object, new VerificationCodeEntityMapper());
    }
}
