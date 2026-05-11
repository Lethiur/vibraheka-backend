using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Logging;
using Moq;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Mappers;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.UserCodeRepositoryTest;

public abstract class GenericUserCodeRepositoryTest
{
    protected Mock<IDynamoDBContext> ContextMock;
    protected Mock<IAmazonDynamoDB> ClientMock;
    protected AWSConfig ConfigMock;
    protected UserCodeRepository Repository;

    [SetUp]
    public void SetUp()
    {
        ContextMock = new Mock<IDynamoDBContext>();
        ClientMock = new Mock<IAmazonDynamoDB>();
        ConfigMock = new AWSConfig
        {
            UserCodesTable = "UserCodesTable"
        };

        Repository = new UserCodeRepository(
            ConfigMock,
            ContextMock.Object,
            ClientMock.Object,
            new UsersCodeMapper(),
            new Mock<ILogger<GenericDynamoRepository<UserCodeDBModel>>>().Object);
    }
}
