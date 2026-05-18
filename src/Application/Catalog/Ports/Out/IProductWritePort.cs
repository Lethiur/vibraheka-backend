using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Domain.Catalog.Entities;

namespace VibraHeka.Application.Catalog.Ports.Out;

public interface IProductWritePort
{
    ITransactionalWriteOperation CreateProduct(ProductEntity product);
}
