using Amazon.DynamoDBv2.DataModel;
using Moq;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.UserRepositoryTest;

public abstract class GenericUserRepositoryTest
{
    
    protected Mock<IDynamoDBContext> ContextMock;
    protected AWSConfig ConfigMock;
    protected UserRepository Repository;

    [SetUp]
    public void SetUp()
    {
        ContextMock = new Mock<IDynamoDBContext>();
        ConfigMock = new AWSConfig()
        {
            UsersTable = "TestUsersTable"
        };

        Repository = new UserRepository(ContextMock.Object, ConfigMock);
    }
}
