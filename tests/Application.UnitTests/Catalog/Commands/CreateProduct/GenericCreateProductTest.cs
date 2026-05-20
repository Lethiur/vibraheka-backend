using Moq;
using NMoneys;
using NUnit.Framework;
using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Application.Catalog.Commands.CreateProduct;
using VibraHeka.Application.Catalog.Models;
using VibraHeka.Application.Catalog.Ports.Out;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Common.Interfaces;

namespace VibraHeka.Application.UnitTests.Catalog.Commands.CreateProduct;

public abstract class GenericCreateProductTest
{
    protected Mock<IProductWritePort> ProductWritePortMock = default!;
    protected Mock<IProductCreationWritePort> ProductCreationWritePortMock = default!;
    protected Mock<ISellableItemPriceWritePort> SellableItemPriceWritePortMock = default!;
    protected Mock<ISellableItemWritePort> SellableItemWritePortMock = default!;
    protected Mock<ICurrentUserService> CurrentUserServiceMock = default!;
    protected Mock<IAtomicWriteStore> TransactionStoreMock = default!;
    protected CreateProductCommandHandler Handler = default!;

    protected const string TestUserId = "test-admin-user-id";

    [SetUp]
    public virtual void SetUp()
    {
        ProductWritePortMock = new Mock<IProductWritePort>();
        ProductCreationWritePortMock = new Mock<IProductCreationWritePort>();
        SellableItemPriceWritePortMock = new Mock<ISellableItemPriceWritePort>();
        SellableItemWritePortMock = new Mock<ISellableItemWritePort>();
        CurrentUserServiceMock = new Mock<ICurrentUserService>();
        TransactionStoreMock = new Mock<IAtomicWriteStore>();

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(TestUserId);

        ProductWritePortMock
            .Setup(x => x.CreateProduct(It.IsAny<ProductEntity>()))
            .Returns(new Mock<ITransactionalWriteOperation>().Object);

        SellableItemWritePortMock
            .Setup(x => x.CreateSellableItem(It.IsAny<SellableItemEntity>()))
            .Returns(new Mock<ITransactionalWriteOperation>().Object);

        SellableItemPriceWritePortMock
            .Setup(x => x.CreateSellableItemPrice(It.IsAny<SellableItemPriceEntity>()))
            .Returns(new Mock<ITransactionalWriteOperation>().Object);

        Handler = new CreateProductCommandHandler(
            ProductWritePortMock.Object,
            ProductCreationWritePortMock.Object,
            SellableItemPriceWritePortMock.Object,
            SellableItemWritePortMock.Object,
            CurrentUserServiceMock.Object,
            TransactionStoreMock.Object);
    }

    protected static CreateProductCommand BuildValidCommand() =>
        new CreateProductCommand(
            Name: "Meditacion Matutina",
            Description: "Sesion de meditacion guiada para el inicio del dia",
            Price: 9.99m,
            CurrencyCode: CurrencyIsoCode.EUR);

    protected static ProductGatewayCreatedResponseModel BuildGatewaySuccessResponse(
        string productGatewayId = "prod_test_abc123",
        string priceGatewayId = "price_test_xyz456") =>
        new ProductGatewayCreatedResponseModel
        {
            ProductGatewayID = productGatewayId,
            ProductGatewayPriceID = priceGatewayId,
        };
}

