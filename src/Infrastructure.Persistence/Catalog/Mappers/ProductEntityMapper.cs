using Infrastructure.Persistence.Catalog.Models;
using Riok.Mapperly.Abstractions;
using VibraHeka.Domain.Catalog.Entities;

namespace Infrastructure.Persistence.Catalog.Mappers;

[Mapper]
public partial class ProductEntityMapper
{
    public partial ProductDBModel FromDomain(ProductEntity entity);

    public partial ProductEntity ToDomain(ProductDBModel entity);
}
