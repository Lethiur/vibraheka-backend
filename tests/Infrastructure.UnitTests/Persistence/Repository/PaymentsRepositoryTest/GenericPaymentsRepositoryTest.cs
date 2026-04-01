using Amazon.SimpleSystemsManagement;
using Microsoft.Extensions.Logging;
using Moq;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.PaymentsRepositoryTest;

public abstract class GenericPaymentsRepositoryTest
{
    protected PaymentsRepository Repository;
    protected Mock<IAmazonSimpleSystemsManagement> SystemsManagementMock;

    [SetUp]
    public void SetUp()
    {
        SystemsManagementMock = new Mock<IAmazonSimpleSystemsManagement>();

        StripeConfig config = new()
        {
            SecretKey = "sk_test",
            PaymentSuccessUrl = "https://success.test",
            PaymentCancelUrl = "https://cancel.test",
            PaymentMethodsAccepted = ["card"],
            SubscriptionID = "price_1"
        };

        AWSConfig awsConfig = new()
        {
            SettingsNameSpace = "VibraHeka",
            Profile = "Twingers",
            Location = "eu-west-1",
            ClientId = "client-id",
            UserPoolId = "user-pool-id",
            EmailTemplatesBucketName = "bucket",
            UsersTable = "users",
            CodesTable = "codes",
            UserCodesTable = "user-codes",
            EmailTemplatesTable = "templates",
            ActionLogTable = "action-log",
            SubscriptionTable = "subscription",
            SubscriptionUserIdIndex = "user-index"
        };

        Repository = new PaymentsRepository(
            config,
            awsConfig,
            SystemsManagementMock.Object,
            new Mock<ILogger<PaymentsRepository>>().Object);
    }
}
