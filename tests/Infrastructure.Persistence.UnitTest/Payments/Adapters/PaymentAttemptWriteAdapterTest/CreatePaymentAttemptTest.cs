using System.ComponentModel;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Payments.Models;
using Moq;
using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Domain.Payments.Entities;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Payments.Adapters.PaymentAttemptWriteAdapterTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class CreatePaymentAttemptTest : GenericPaymentAttemptWriteAdapterTest
{
    [Test]
    [DisplayName("Should create transactional write with correct table, add the mapped model once, and return a non-null operation wrapper")]
    public void ShouldCreateTransactionalWriteOperationWithCorrectTableAndMappedModel()
    {
        // Given: a valid PaymentAttemptEntity with known IDs
        PaymentAttemptEntity paymentAttempt = BuildDefaultPaymentAttemptEntity();

        // When: CreatePaymentAttempt is called on the adapter
        ITransactionalWriteOperation result = Adapter.CreatePaymentAttempt(paymentAttempt);

        // Then: AddSaveItem is called once with the model produced by the mapper
        TransactWriteMock.Verify(
            x => x.AddSaveItem(
                It.Is<PaymentAttemptDBModel>(m =>
                    m.PaymentAttemptID == paymentAttempt.PaymentAttemptID &&
                    m.OrderId == paymentAttempt.OrderId &&
                    m.UserId == paymentAttempt.UserId)),
            Times.Once,
            $"Expected AddSaveItem called once with PaymentAttemptDBModel where PaymentAttemptID='{paymentAttempt.PaymentAttemptID}'");

        // Then: the returned operation is a non-null DynamoTransactionalWriteOperation
        Assert.That(result, Is.Not.Null,
            "Expected a non-null ITransactionalWriteOperation");
        Assert.That(result, Is.InstanceOf<DynamoTransactionalWriteOperation>(),
            $"Expected DynamoTransactionalWriteOperation but got '{result.GetType().Name}'");

        DynamoTransactionalWriteOperation dynamoOperation = (DynamoTransactionalWriteOperation)result;
        Assert.That(dynamoOperation.Item, Is.SameAs(TransactWriteMock.Object),
            "Expected the wrapped ITransactWrite item to be the mock returned by CreateTransactWrite");

        ContextMock.VerifyNoOtherCalls();
        TransactWriteMock.VerifyNoOtherCalls();
    }
}

