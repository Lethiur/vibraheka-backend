using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
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
        CredentialProfileStoreChain profileStore = new CredentialProfileStoreChain();
        if (!profileStore.TryGetAWSCredentials(_configuration.Profile, out AWSCredentials credentials))
        {
            throw new InvalidOperationException(
                $"AWS profile '{_configuration.Profile}' was not found in the shared credentials/config files.");
        }

        S3 = new AmazonS3Client(credentials, new AmazonS3Config
        {
            RegionEndpoint = region
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

