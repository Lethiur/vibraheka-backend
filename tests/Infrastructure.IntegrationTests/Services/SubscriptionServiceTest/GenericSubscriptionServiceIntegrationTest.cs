using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using VibraHeka.Domain.Common.Interfaces.Orders;
using VibraHeka.Domain.Common.Interfaces.Payments;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Mappers;
using VibraHeka.Infrastructure.Persistence.Repository;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.SubscriptionServiceTest;

public class SuccessPaymentRepositoryStub : IPaymentRepository
{
    public Task<Result<SubscriptionCheckoutSessionEntity>> InitiateSubscriptionPaymentAsync(UserEntity payer, SubscriptionEntity orderEntity, CancellationToken cancellationToken)
        => Task.FromResult(Result.Success(new SubscriptionCheckoutSessionEntity()
        {
            Url = "https://checkout.test",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1),
        }));

    public Task<Result<string>> GetSubscriptionPanelUrlAsync(UserEntity payer, CancellationToken cancellationToken)
        => Task.FromResult(Result.Success("https://portal.test"));

    public Task<Result<string>> RegisterCustomerAsync(UserEntity user, CancellationToken cancellationToken)
        => Task.FromResult(Result.Success("cus_test"));

    public Task<Result<Unit>> CancelSubscriptionForUser(SubscriptionEntity subscription, CancellationToken cancellationToken)
        => Task.FromResult(Result.Success(Unit.Value));

    public Task<Result<Unit>> ReactivateSubscriptionForUser(SubscriptionEntity entity, CancellationToken cancellationToken)
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
            LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<SubscriptionRepository>());

        _service = new SubscriptionService(
            _subscriptionRepository,
            new SuccessPaymentRepositoryStub(),
            _stripeConfig,
            LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<SubscriptionService>());
    }

    [OneTimeTearDown]
    public void OneTimeTearDownChild()
    {
        _dynamoDbContext.Dispose();
    }
}
