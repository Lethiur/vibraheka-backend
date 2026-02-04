using Amazon.DynamoDBv2.DataModel;
using Moq;
using VibraHeka.Infrastructure.Entities;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.DynamoRepositoryTest;

public abstract class GenericDynamoRepositoryTest
{
    protected Mock<IDynamoDBContext> _contextMock;
    protected AWSConfig _configMock;
    protected TestableDynamoRepository _repository;
    protected const string TableName = "RealTableName";
    
    [SetUp]
    public void SetUp()
    {
        _contextMock = new Mock<IDynamoDBContext>();
        _configMock = new AWSConfig();
        _repository = new TestableDynamoRepository(_contextMock.Object, TableName);
    }
}
