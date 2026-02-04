using Amazon;
using Amazon.S3;
using Moq;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Persistence.S3;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.S3.EmailTemplateStorageRepositoryTest;

public abstract class GenericEmailTemplateStorageRepositoryTest
{
    protected Mock<IAmazonS3> ClientMock = default!;
    protected AWSConfig Options = default!;
    protected EmailTemplateStorageRepository Repository = default!;
    protected CancellationToken TestCancellationToken;

    [SetUp]
    public void SetUp()
    {
        ClientMock = new Mock<IAmazonS3>(MockBehavior.Loose);
        ClientMock.Setup(c => c.Config).Returns(new AmazonS3Config { RegionEndpoint = RegionEndpoint.USEast1 });

        Options = new AWSConfig
        {
            EmailTemplatesBucketName = "unit-test-bucket"
        };

        Repository = new EmailTemplateStorageRepository(ClientMock.Object, Options);
        TestCancellationToken = CancellationToken.None;
    }
}

