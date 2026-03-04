using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Infrastructure.UnitTests.Services.PaymentServiceTest;

[TestFixture]
public class CancelSubscriptionPaymentTest : GenericPaymentServiceTest
{
    [Test]
    public async Task ShouldNotAllowNullEntity()
    {
        // When: Service is invoked with null entity
        Result<Unit> result = await _service.CancelSubscriptionPayment(null!, CancellationToken.None);
        
        // Then: Should return failure
        Assert.That(result.IsFailure, Is.True);
        
        // And: Should have specific error message
        Assert.That(result.Error, Is.EqualTo(SubscriptionErrors.ErrorWhileSubscribing));
    }

    [Test]
    public async Task ShouldContainExceptionFromRepository()
    {
        // Given: Some mocking
        _paymentRepositoryMock.Setup(repository => repository.CancelSubscriptionPayment(It.IsAny<SubscriptionCheckoutSessionEntity>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));
        
        // When: Service is invoked
        Result<Unit> result = await _service.CancelSubscriptionPayment(new SubscriptionCheckoutSessionEntity(), CancellationToken.None);
        
        // Then: Should return failure
        Assert.That(result.IsFailure, Is.True);
        
        // And: Should have specific error message
        Assert.That(result.Error, Is.EqualTo("Database error"));
    }

    [Test]
    public async Task ShouldInvokeRepositoryJustFine()
    {
        // Given: Some mocking
        _paymentRepositoryMock.Setup(repository => repository.CancelSubscriptionPayment(It.IsAny<SubscriptionCheckoutSessionEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);
        
        // When: Service is invoked
        Result<Unit> result = await _service.CancelSubscriptionPayment(new SubscriptionCheckoutSessionEntity(), CancellationToken.None);
        
        // Then: Should return success
        Assert.That(result.IsSuccess, Is.True);
        
        // And: The mock should have been invoked once
        _paymentRepositoryMock.Verify(repository => repository.CancelSubscriptionPayment(It.IsAny<SubscriptionCheckoutSessionEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
