using Amazon.DynamoDBv2.DataModel;
using Infrastructure.Persistence.Payments.Adapters;
using Infrastructure.Persistence.Payments.Mappers;
using Infrastructure.Persistence.Payments.Models;
using Moq;
using VibraHeka.Domain.Payments.Entities;

namespace VibraHeka.Infrastructure.Persistence.UnitTest.Payments.Adapters.PaymentAttemptWriteAdapterTest;

public abstract class GenericPaymentAttemptWriteAdapterTest
{
    protected Mock<IDynamoDBContext> ContextMock = default!;
    protected Mock<ITransactWrite<PaymentAttemptDBModel>> TransactWriteMock = default!;
    protected PaymentAttemptMapper Mapper = default!;
    protected PaymentAttemptWriteAdapter Adapter = default!;

    [SetUp]
    public virtual void SetUp()
    {
        ContextMock = new Mock<IDynamoDBContext>();
        TransactWriteMock = new Mock<ITransactWrite<PaymentAttemptDBModel>>();
        Mapper = new PaymentAttemptMapper();
        Adapter = new PaymentAttemptWriteAdapter(Mapper, ContextMock.Object);

        ContextMock
            .Setup(x => x.CreateTransactWrite<PaymentAttemptDBModel>())
            .Returns(TransactWriteMock.Object);
    }

    protected static PaymentAttemptEntity BuildDefaultPaymentAttemptEntity() =>
        new()
        {
            PaymentAttemptID = "attempt-unit-test-001",
            OrderId = "order-unit-test-001",
            UserId = "user-unit-test-001",
            PaymentGatewayCheckoutURL = "https://checkout.stripe.com/test"
        };
}

