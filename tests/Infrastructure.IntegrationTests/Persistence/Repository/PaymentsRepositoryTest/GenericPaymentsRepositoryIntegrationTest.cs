using Microsoft.Extensions.Logging;
using VibraHeka.Domain.Common.Interfaces.Payments;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.PaymentsRepositoryTest;

public abstract class GenericPaymentsRepositoryIntegrationTest : TestBase
{
    protected IPaymentRepository _repository;

    [OneTimeSetUp]
    public void OneTimeSetUpChild()
    {
        base.OneTimeSetUp();
        _repository = new PaymentsRepository(_stripeConfig, CreateTestLogger<PaymentsRepository>());
    }
}

