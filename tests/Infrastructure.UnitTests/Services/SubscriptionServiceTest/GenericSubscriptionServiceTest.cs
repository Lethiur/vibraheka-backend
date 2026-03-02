using Microsoft.Extensions.Logging;
using Moq;
using VibraHeka.Domain.Common.Interfaces.Orders;
using VibraHeka.Domain.Common.Interfaces.Payments;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.UnitTests.Services.SubscriptionServiceTest;

public abstract class GenericSubscriptionServiceTest
{
    protected Mock<ISubscriptionRepository> _subscriptionRepositoryMock;
    protected Mock<IPaymentRepository> _paymentRepositoryMock;
    protected Mock<ILogger<SubscriptionService>> _loggerMock;
    protected StripeConfig _stripeConfig;
    protected SubscriptionService _service;

    [SetUp]
    public void SetUp()
    {
        _subscriptionRepositoryMock = new Mock<ISubscriptionRepository>();
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _loggerMock = new Mock<ILogger<SubscriptionService>>();
        _stripeConfig = new StripeConfig { SubscriptionID = "price-123" };

        _service = new SubscriptionService(
            _subscriptionRepositoryMock.Object,
            _paymentRepositoryMock.Object,
            _stripeConfig,
            _loggerMock.Object);
    }
}
