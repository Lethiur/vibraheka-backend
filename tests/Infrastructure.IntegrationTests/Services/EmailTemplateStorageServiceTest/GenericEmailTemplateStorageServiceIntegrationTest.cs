using Amazon;
using Amazon.S3;
using VibraHeka.Infrastructure.Persistence.S3;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.EmailTemplateStorageServiceTest;

public abstract class GenericEmailTemplateStorageServiceIntegrationTest : TestBase
{
    protected IAmazonS3 S3 = default!;
    protected EmailTemplateStorageRepository Repository = default!;
    protected EmailTemplateStorageService Service = default!;

    protected CancellationToken TestCancellationToken;
    protected string BucketName = default!;

    [OneTimeSetUp]
    public void OneTimeSetUpStorage()
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
        Service = new EmailTemplateStorageService(Repository);
    }

    [OneTimeTearDown]
    public void OneTimeTearDownStorage()
    {
        S3?.Dispose();
    }
}

