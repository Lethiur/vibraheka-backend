using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Subscriptions.Entities;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.SubscriptionServiceTest;

public class SuccessPaymentRepositoryStub : IPaymentRepository
{
    public Task<Result<SubscriptionCheckoutSessionEntity>> InitiateSubscriptionPaymentAsync(UserProfileEntity payer,
        CancellationToken cancellationToken)
        => Task.FromResult(Result.Success(new SubscriptionCheckoutSessionEntity()
        {
            Url = "https://checkout.test",
            SessionExpiresAt = DateTimeOffset.UtcNow.AddDays(1),
            CheckoutSessionID = "cs_test",
            InternalReferenceID = "ref_test",
        }));

    public Task<Result<string>> GetSubscriptionPanelUrlAsync(UserProfileEntity payer, CancellationToken cancellationToken)
        => Task.FromResult(Result.Success("https://portal.test"));

    public Task<Result<string>> RegisterCustomerAsync(UserProfileEntity userProfile, CancellationToken cancellationToken)
        => Task.FromResult(Result.Success("cus_test"));

    public Task<Result<Unit>> CancelSubscriptionForUser(SubscriptionEntity subscription, CancellationToken cancellationToken)
        => Task.FromResult(Result.Success(Unit.Value));

    public Task<Result<Unit>> ReactivateSubscriptionForUser(SubscriptionEntity entity, CancellationToken cancellationToken)
        => Task.FromResult(Result.Success(Unit.Value));

    public Task<Result<Unit>> CancelSubscriptionPayment(SubscriptionCheckoutSessionEntity entity, CancellationToken cancellationToken)
        => Task.FromResult(Result.Success(Unit.Value));
}

public abstract class GenericSubscriptionServiceIntegrationTest : TestBase
{
    protected IDynamoDBContext _dynamoDbContext;
    protected ISubscriptionRepository _subscriptionRepository;
    protected ISubscriptionService _service;

    [OneTimeSetUp]
    public void OneTimeSetUpChild()
    {
        base.OneTimeSetUp();
        _dynamoDbContext = CreateDynamoDBContext();
        _subscriptionRepository = new SubscriptionRepository(
            _configuration,
            _dynamoDbContext,
            new SubscriptionEntityMapper(),
            CreateTestLogger<SubscriptionRepository>());

        _service = new SubscriptionService(
            _subscriptionRepository,
            new SuccessPaymentRepositoryStub(),
            _stripeConfig,
            CreateTestLogger<SubscriptionService>());
    }

    [OneTimeTearDown]
    public void OneTimeTearDownChild()
    {
        _dynamoDbContext.Dispose();
    }
}

