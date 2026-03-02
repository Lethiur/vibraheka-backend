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
    protected AWSConfig ConfigMock;
    protected UserCodeRepository Repository;

    [SetUp]
    public void SetUp()
    {
        ContextMock = new Mock<IDynamoDBContext>();
        ConfigMock = new AWSConfig
        {
            UserCodesTable = "UserCodesTable"
        };

        Repository = new UserCodeRepository(
            ConfigMock,
            ContextMock.Object,
            new UsersCodeMapper(),
            new Mock<ILogger<GenericDynamoRepository<UserCodeDBModel>>>().Object);
    }
}
