using Microsoft.Extensions.Logging;
using Moq;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.PaymentsRepositoryTest;

public abstract class GenericPaymentsRepositoryTest
{
    protected PaymentsRepository Repository;

    [SetUp]
    public void SetUp()
    {
        StripeConfig config = new()
        {
            SecretKey = "sk_test",
            PaymentSuccessUrl = "https://success.test",
            PaymentCancelUrl = "https://cancel.test",
            PaymentMethodsAccepted = ["card"],
            SubscriptionID = "price_1"
        };

        Repository = new PaymentsRepository(config, new Mock<ILogger<PaymentsRepository>>().Object);
    }
}
