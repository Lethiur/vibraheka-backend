using Amazon;
using Amazon.S3;
using VibraHeka.Infrastructure.Persistence.S3;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.S3.EmailTemplateStorageRepositoryTest;

public abstract class GenericEmailTemplateStorageRepositoryIntegrationTest : TestBase
{
    protected IAmazonS3 S3 = null!;
    protected EmailTemplateStorageRepository Repository = default!;
    protected CancellationToken TestCancellationToken;
    protected string BucketName = default!;

    [OneTimeSetUp]
    public void SetupS3()
    {
        base.OneTimeSetUp();
        RegionEndpoint? region = RegionEndpoint.GetBySystemName(_configuration.Location);

        S3 = new AmazonS3Client(new AmazonS3Config
        {
            RegionEndpoint = region,
            Profile = new Profile(_configuration.Profile)
        });

        BucketName = _configuration.EmailTemplatesBucketName;
        TestCancellationToken = CancellationToken.None;
        Repository = new EmailTemplateStorageRepository(S3, _configuration);
    }

    [OneTimeTearDown]
    public void TearDownS3()
    {
        S3?.Dispose();
    }
}

