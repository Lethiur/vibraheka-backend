using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Stripe.Checkout;
using Stripe.Tax;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Common.Interfaces.Orders;
using VibraHeka.Domain.Common.Interfaces.Payments;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Mappers;
using VibraHeka.Infrastructure.Persistence.Repository;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.PaymentServiceTest;

[TestFixture]
public class RegisterOrderTest : TestBase
{
    
    private IPaymentService _paymentService;

    private IPaymentRepository _paymentRepository;
    
    private ISubscriptionRepository _subscriptionRepository;
    
    private IUserRepository _userRepository;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        base.OneTimeSetUp();
        _userRepository = new UserRepository(CreateDynamoDBContext(), _configuration);
        _paymentRepository = new PaymentsRepository(_stripeConfig);
        _subscriptionRepository = new SubscriptionRepository( _configuration, CreateDynamoDBContext(), new SubscriptionEntityMapper(), LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<SubscriptionRepository>());
        _paymentService = new PaymentService(_paymentRepository, _subscriptionRepository, _userRepository);
    }
    
    
    [Test]
    public async Task ShouldGenerateCheckoutUrlProperly()
    {
        // Given: An User
        UserEntity userEntity = CreateValidUser();
        await _userRepository.AddAsync(userEntity);
        
        // When: Subscribing the user
        Result<string> result = await _paymentService.RegisterSubscriptionAsync(userEntity.Id, new SubscriptionEntity()
        {
            ExternalSubscriptionItemID = _stripeConfig.SubscriptionID,
            SubscriptionStatus = SubscriptionStatus.Created,
            UserID = userEntity.Id,
            SubscriptionID = Guid.NewGuid().ToString(),
        }, CancellationToken.None);
        
        // Then: The url should be there.
        if (result.IsFailure)
        {
            Assert.Fail(result.Error);
        }
        
        Assert.That(result.IsSuccess, Is.True);
        
        // And: URL should be valid
        string url = result.Value;
        Assert.That(url, Is.Not.Null);
        Assert.That(url, Is.Not.Empty);
        Assert.That(url.StartsWith("https://checkout.stripe.com/c/pay/cs_"), Is.True);
        
        // And: Order should be registered correctly
        AssertSessionBasedOnUrl(url, userEntity);
    }

    [Test]
    public async Task ShouldReturnUrlWhenUserHasCustomerID()
    {
         // Given: An User
        UserEntity userEntity = CreateValidUser();

        Result<string> registerCustomerAsync = await _paymentRepository.RegisterCustomerAsync(userEntity, CancellationToken.None);
        userEntity.CustomerID = registerCustomerAsync.Value;
        
        await _userRepository.AddAsync(userEntity);
        
        // When: Subscribing the user
        Result<string> result = await _paymentService.RegisterSubscriptionAsync(userEntity.Id, new SubscriptionEntity()
        {
            ExternalSubscriptionItemID = _stripeConfig.SubscriptionID,
            SubscriptionStatus = SubscriptionStatus.Created,
            UserID = userEntity.Id,
            SubscriptionID = Guid.NewGuid().ToString(),
        }, CancellationToken.None);
        
        // Then: The url should be there.
        if (result.IsFailure)
        {
            Assert.Fail(result.Error);
        }
        
        Assert.That(result.IsSuccess, Is.True);
        
        // And: URL should be valid
        string url = result.Value;
        Assert.That(url, Is.Not.Null);
        Assert.That(url, Is.Not.Empty);
        Assert.That(url.StartsWith("https://checkout.stripe.com/c/pay/cs_"), Is.True);
        
        
        // And: Order should be registered correctly
        AssertSessionBasedOnUrl(url, userEntity);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("      ")]
    [Test]
    public async Task ShouldReturnErrorWhenUserIDIsInvalid(string? invalidUserId)
    {
        // When: Service is invoked with invalid user id
        Result<string> registerSubscriptionAsync = await _paymentService.RegisterSubscriptionAsync(invalidUserId!, new SubscriptionEntity(), CancellationToken.None);
        
        // Then: The result should be in a failure state
        Assert.That(registerSubscriptionAsync.IsFailure, Is.True);
        
        // And: Error should be the expected one
        Assert.That(registerSubscriptionAsync.Error, Is.EqualTo(UserErrors.InvalidUserID));
    }


    [Test]
    public async Task ShouldReturnErrorWhenTheUserDoesNotExist()
    {
        // When: Service is invoked with invalid user id
        Result<string> registerSubscriptionAsync = await _paymentService.RegisterSubscriptionAsync(Guid.NewGuid().ToString(), new SubscriptionEntity(), CancellationToken.None);
        
        // Then: The result should be in a failure state
        Assert.That(registerSubscriptionAsync.IsFailure, Is.True);
        
        // And: Error should be the expected one
        Assert.That(registerSubscriptionAsync.Error, Is.EqualTo(UserErrors.UserNotFound));
    }
    
    
    public static string ExtractSessionId(string checkoutUrl)
    {
        Uri uri = new Uri(checkoutUrl);

        // Example path: /c/pay/cs_test_ABC123...
        string[] segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // Find the "cs_" segment (Stripe Checkout Session id)
        foreach (string s in segments)
        {
            if (s.StartsWith("cs_", StringComparison.OrdinalIgnoreCase))
                return s;
        }

        throw new InvalidOperationException("No Checkout Session id (cs_...) found in URL path.");
    }

    private void AssertSessionBasedOnUrl(string url, UserEntity userEntity)
    {
        SessionService service = new SessionService();
        Session? session = service.Get(ExtractSessionId(url), new SessionGetOptions
        {
            Expand = new List<string>
            {
                "customer",
                "line_items",
                "payment_intent",
                "subscription"
            }
        });
        using (Assert.EnterMultipleScope())
        {
            Assert.That(session.Url, Is.EqualTo(url));
            Assert.That(session.Customer.Email, Is.EqualTo(userEntity.Email));
            Assert.That(session.Customer.Name, Is.EqualTo(userEntity.FirstName + " " + userEntity.MiddleName + " " + userEntity.LastName));
            Assert.That(session.Customer.Phone, Is.EqualTo(userEntity.PhoneNumber));
            Assert.That(session.Mode, Is.EqualTo("subscription"));
            Assert.That(session.SuccessUrl, Is.EqualTo(_stripeConfig.PaymentSuccessUrl));
            Assert.That(session.CancelUrl, Is.EqualTo(_stripeConfig.PaymentCancelUrl));
            Assert.That(session.LineItems!.Data, Is.Not.Null);
            Assert.That(session.LineItems.Data, Has.Count.EqualTo(1));
            Assert.That(session.LineItems.Data[0].Price.Id, Is.EqualTo(_stripeConfig.SubscriptionID));
        }
    }
}
