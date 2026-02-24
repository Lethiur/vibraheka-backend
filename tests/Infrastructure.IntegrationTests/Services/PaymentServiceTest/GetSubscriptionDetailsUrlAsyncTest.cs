using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Interfaces.Payments;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.Repository;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.PaymentServiceTest;

[TestFixture]
public class GetSubscriptionDetailsUrlAsyncTest : TestBase
{
    private IPaymentService _paymentService;
    private IPaymentRepository _paymentRepository;
    private IUserRepository _userRepository;

    [OneTimeSetUp]
    public void OneTimeSetUpChild()
    {
        base.OneTimeSetUp();
        _userRepository = new UserRepository(CreateDynamoDBContext(), _configuration);
        _paymentRepository = new PaymentsRepository(_stripeConfig, LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<PaymentsRepository>());
        _paymentService = new PaymentService(_paymentRepository, _userRepository);
    }

    [Test]
    public async Task ShouldReturnBillingPortalUrlWhenUserExistsWithCustomerId()
    {
        // Given
        UserEntity userEntity = CreateValidUser();
        Result<string> registerCustomerResult = await _paymentRepository.RegisterCustomerAsync(userEntity, CancellationToken.None);
        userEntity.CustomerID = registerCustomerResult.Value;
        await _userRepository.AddAsync(userEntity);

        // When
        Result<string> result = await _paymentService.GetSubscriptionDetailsUrlAsync(userEntity.Id, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null.And.Not.Empty);
        Assert.That(result.Value.StartsWith("https://billing.stripe.com/"), Is.True);
    }

    [Test]
    public async Task ShouldReturnUserNotFoundWhenUserDoesNotExist()
    {
        // Given
        string nonExistentUserId = Guid.NewGuid().ToString();

        // When
        Result<string> result = await _paymentService.GetSubscriptionDetailsUrlAsync(nonExistentUserId, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UserNotFound));
    }
}
