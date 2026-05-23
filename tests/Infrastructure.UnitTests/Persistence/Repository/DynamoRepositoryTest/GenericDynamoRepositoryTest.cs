using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Internal.Entities;
using Moq;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.DynamoRepositoryTest;

public abstract class GenericDynamoRepositoryTest
{
    protected Mock<IDynamoDBContext> _contextMock;
    protected Mock<IAmazonDynamoDB> _dynamoDbClientMock;
    protected TestableDynamoRepository _repository;
    protected const string TableName = "RealTableName";

    [SetUp]
    public void SetUp()
    {
        AWSXRayRecorder.Instance.TraceContext.SetEntity(new Segment("mock"));
        _contextMock = new Mock<IDynamoDBContext>();
        _dynamoDbClientMock = new Mock<IAmazonDynamoDB>();
        _repository = new TestableDynamoRepository(_contextMock.Object, _dynamoDbClientMock.Object);
    }
}
