namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.PaymentsRepositoryTest;

public abstract class GenericPaymentsRepositoryIntegrationTest : TestBase
{
    protected IPaymentRepository _repository;

    [OneTimeSetUp]
    public void OneTimeSetUpChild()
    {
        base.OneTimeSetUp();
        _repository = new PaymentsRepository(
            _stripeConfig,
            _configuration,
            CreateSystemsManagementClient(),
            CreateTestLogger<PaymentsRepository>());
    }
}
