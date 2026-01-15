using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Configuration;
using Moq;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.DynamoRepositoryTest;

public abstract class GenericDynamoRepositoryTest
{
    protected Mock<IDynamoDBContext> _contextMock;
    private Mock<IConfiguration> _configMock;
    protected TestableDynamoRepository _repository;
    private const string TableKey = "Dynamo:TestTable";
    protected const string TableName = "RealTableName";
    
    [SetUp]
    public void SetUp()
    {
        _contextMock = new Mock<IDynamoDBContext>();
        _configMock = new Mock<IConfiguration>();
        _configMock.Setup(c => c[TableKey]).Returns(TableName);
        _repository = new TestableDynamoRepository(_contextMock.Object, _configMock.Object, TableKey);
    }
}
