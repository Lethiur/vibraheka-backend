using Amazon.DynamoDBv2.DataModel;
using Infrastructure.Persistence.Catalog.Mappers;
using Infrastructure.Persistence.Catalog.Models;
using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Application.Catalog.Ports.Out;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Infrastructure.Entities;

namespace Infrastructure.Persistence.Catalog.Adapters;

/// <summary>
/// Provides functionality to persist product data in the underlying DynamoDB store.
/// </summary>
/// <remarks>
/// The ProductWriteAdapter is responsible for mapping domain product entities to
/// DynamoDB database models and creating transactional write operations to
/// store them in the configured DynamoDB table. Implements the IProductWritePort interface.
/// </remarks>
public class ProductWriteAdapter(ProductEntityMapper Mapper, AWSConfig Config, IDynamoDBContext Context)
    : IProductWritePort
{
    /// <summary>
    /// Creates a product record in the database using a transactional write operation.
    /// </summary>
    /// <param name="product">The product entity containing the details to be stored in the database.</param>
    /// <returns>An instance of <see cref="ITransactionalWriteOperation"/> representing the transactional write operation for the created product.</returns>
    public ITransactionalWriteOperation CreateProduct(ProductEntity product)
    {
        ProductDBModel model = Mapper.FromDomain(product);

        ITransactWrite<ProductDBModel> transaction = Context.CreateTransactWrite<ProductDBModel>(new TransactWriteConfig()
        {
            OverrideTableName = Config.ProductTable
        });
        transaction.AddSaveItem(model);
        return new DynamoTransactionalWriteOperation(transaction);
    }
}
