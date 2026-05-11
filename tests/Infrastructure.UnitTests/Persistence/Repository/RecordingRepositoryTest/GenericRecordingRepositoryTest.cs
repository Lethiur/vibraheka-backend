using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Mappers;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.RecordingRepositoryTest;

public abstract class GenericRecordingRepositoryTest
{
    protected Mock<IDynamoDBContext> ContextMock = default!;
    protected Mock<IAmazonDynamoDB> DynamoDbClientMock = default!;
    protected Mock<ILogger<RecordingRepository>> LoggerMock = default!;
    protected AWSConfig Config = default!;
    protected RecordingRepository Repository = default!;

    [SetUp]
    public void SetUp()
    {
        ContextMock = new Mock<IDynamoDBContext>();
        DynamoDbClientMock = new Mock<IAmazonDynamoDB>();
        LoggerMock = new Mock<ILogger<RecordingRepository>>();
        Config = new AWSConfig { RecordingsTable = "unit-test-recordings-table" };
        Repository = new RecordingRepository(ContextMock.Object, DynamoDbClientMock.Object, Config, new RecordingEntityMapper(), LoggerMock.Object);
    }
}

