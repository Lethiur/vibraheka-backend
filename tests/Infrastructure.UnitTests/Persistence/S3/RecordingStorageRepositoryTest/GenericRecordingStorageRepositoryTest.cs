using Amazon;
using Amazon.S3;
using Moq;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Persistence.S3;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.S3.RecordingStorageRepositoryTest;

public abstract class GenericRecordingStorageRepositoryTest
{
    protected Mock<IAmazonS3> ClientMock = default!;
    protected AWSConfig Config = default!;
    protected RecordingStorageRepository Repository = default!;

    [SetUp]
    public virtual void SetUp()
    {
        ClientMock = new Mock<IAmazonS3>(MockBehavior.Loose);
        ClientMock
            .Setup(c => c.Config)
            .Returns(new AmazonS3Config { RegionEndpoint = RegionEndpoint.USEast1 });

        Config = new AWSConfig { RecordingsBucketName = "unit-test-recordings-bucket" };
        Repository = new RecordingStorageRepository(ClientMock.Object, Config);
    }
}
