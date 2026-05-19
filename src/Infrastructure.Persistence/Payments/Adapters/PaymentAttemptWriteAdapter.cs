using Amazon.DynamoDBv2.DataModel;
using Infrastructure.Persistence.Payments.Mappers;
using Infrastructure.Persistence.Payments.Models;
using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Application.Payments.Ports.Out;
using VibraHeka.Domain.Payments.Entities;
using VibraHeka.Infrastructure.Entities;

namespace Infrastructure.Persistence.Payments.Adapters;

/// <summary>
/// The PaymentAttemptWriteAdapter class provides an implementation of the
/// IPaymentAttemptWritePort interface. It is responsible for creating
/// and managing transactional write operations for payment attempts,
/// leveraging Amazon DynamoDB as the persistence layer.
/// </summary>
public class PaymentAttemptWriteAdapter(PaymentAttemptMapper Mapper, AWSConfig Config, IDynamoDBContext Context)
    : IPaymentAttemptWritePort
{
    /// <summary>
    /// Creates a payment intent and returns a transactional write operation.
    /// </summary>
    /// <param name="paymentIntent">The payment intent entity containing the details for the payment attempt to be created.</param>
    /// <returns>An instance of <see cref="ITransactionalWriteOperation"/> representing the transactional write operation for the payment attempt.</returns>
    public ITransactionalWriteOperation CreatePaymentAttempt(PaymentAttemptEntity paymentIntent)
    {
        PaymentAttemptDBModel model = Mapper.FromDomain(paymentIntent);

        ITransactWrite<PaymentAttemptDBModel> transaction = Context.CreateTransactWrite<PaymentAttemptDBModel>(new TransactWriteConfig()
        {
            OverrideTableName = Config.PaymentAttemptTable
        });
        transaction.AddSaveItem(model);

        return new DynamoTransactionalWriteOperation(transaction);
    }
}
