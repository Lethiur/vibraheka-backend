using System.ComponentModel;
using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Application.Catalog.Commands.CreateProduct;
using VibraHeka.Application.Catalog.Models;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Errors;

namespace VibraHeka.Application.UnitTests.Catalog.Commands.CreateProduct;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class CreateProductCommandHandlerTest : GenericCreateProductTest
{
    [Test]
    [DisplayName("Should return CAT-005 failure when gateway creation fails")]
    public async Task ShouldReturnFailureWhenGatewayCreationFails()
    {
        // Given: a valid command and the gateway port returns failure
        CreateProductCommand command = BuildValidCommand();

        ProductCreationWritePortMock
            .Setup(x => x.CreateProductInGatewayAsync(
                It.IsAny<ProductEntity>(),
                It.IsAny<SellableItemPriceEntity>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<ProductGatewayCreatedResponseModel>("S-003"));

        // When: the handler processes the command
        Result<string> result = await Handler.Handle(command, CancellationToken.None);

        // Then: result should be failure with CAT-005
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure when gateway creation fails but got success with value: '{(result.IsSuccess ? result.Value : "N/A")}'");
        Assert.That(result.Error, Is.EqualTo(CatalogErrors.FailedToCreateProduct),
            $"Expected error '{CatalogErrors.FailedToCreateProduct}' but got '{result.Error}'");

        ProductCreationWritePortMock.Verify(
            x => x.CreateProductInGatewayAsync(
                It.Is<ProductEntity>(e =>
                    e.Name == command.Name &&
                    e.Description == command.Description &&
                    !string.IsNullOrEmpty(e.ProductID)),
                It.Is<SellableItemPriceEntity>(p =>
                    p.Amount.Amount == command.Price),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected CreateProductInGatewayAsync to be called once with correct entity data");

        ProductWritePortMock.Verify(
            x => x.CreateProduct(It.Is<ProductEntity>(e => e.Name == command.Name)),
            Times.Never,
            "Expected CreateProduct to never be called when gateway fails (railway short-circuit)");

        SellableItemWritePortMock.Verify(
            x => x.CreateSellableItem(It.Is<SellableItemEntity>(e => e.Name == command.Name)),
            Times.Never,
            "Expected CreateSellableItem to never be called when gateway fails");

        SellableItemPriceWritePortMock.Verify(
            x => x.CreateSellableItemPrice(It.Is<SellableItemPriceEntity>(p => p.Amount.Amount == command.Price)),
            Times.Never,
            "Expected CreateSellableItemPrice to never be called when gateway fails");

        TransactionStoreMock.Verify(
            x => x.CommitAsync(
                It.Is<TransactionalWriteBatch>(b => b.Operations.Count >= 0),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Never,
            "Expected CommitAsync to never be called when gateway creation fails (railway short-circuit)");

        ProductCreationWritePortMock.VerifyNoOtherCalls();
        ProductWritePortMock.VerifyNoOtherCalls();
        SellableItemWritePortMock.VerifyNoOtherCalls();
        SellableItemPriceWritePortMock.VerifyNoOtherCalls();
        TransactionStoreMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return CAT-005 failure when atomic commit fails")]
    public async Task ShouldReturnFailureWhenAtomicCommitFails()
    {
        // Given: a valid command, gateway succeeds but transactional commit fails
        CreateProductCommand command = BuildValidCommand();
        ProductGatewayCreatedResponseModel gatewayResponse = BuildGatewaySuccessResponse();

        ProductCreationWritePortMock
            .Setup(x => x.CreateProductInGatewayAsync(
                It.IsAny<ProductEntity>(),
                It.IsAny<SellableItemPriceEntity>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(gatewayResponse));

        TransactionStoreMock
            .Setup(x => x.CommitAsync(
                It.IsAny<TransactionalWriteBatch>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Unit>("GPE-999"));

        // When: the handler processes the command
        Result<string> result = await Handler.Handle(command, CancellationToken.None);

        // Then: result should be failure with CAT-005
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure when atomic commit fails but got success with value: '{(result.IsSuccess ? result.Value : "N/A")}'");
        Assert.That(result.Error, Is.EqualTo(CatalogErrors.FailedToCreateProduct),
            $"Expected error '{CatalogErrors.FailedToCreateProduct}' but got '{result.Error}'");

        ProductCreationWritePortMock.Verify(
            x => x.CreateProductInGatewayAsync(
                It.Is<ProductEntity>(e => e.Name == command.Name),
                It.Is<SellableItemPriceEntity>(p => p.Amount.Amount == command.Price),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected CreateProductInGatewayAsync to be called exactly once");

        ProductWritePortMock.Verify(
            x => x.CreateProduct(It.Is<ProductEntity>(e =>
                e.Name == command.Name &&
                e.CreatedBy == TestUserId)),
            Times.Once,
            "Expected CreateProduct to be called once with entity matching command and CreatedBy");

        SellableItemWritePortMock.Verify(
            x => x.CreateSellableItem(It.Is<SellableItemEntity>(e =>
                e.Name == command.Name &&
                e.CreatedBy == TestUserId &&
                e.IsActive)),
            Times.Once,
            "Expected CreateSellableItem to be called once with active entity matching command");

        SellableItemPriceWritePortMock.Verify(
            x => x.CreateSellableItemPrice(
                It.Is<SellableItemPriceEntity>(p =>
                    p.ExternalProductID == gatewayResponse.ProductGatewayID &&
                    p.ExternalPriceID == gatewayResponse.ProductGatewayPriceID)),
            Times.Once,
            "Expected CreateSellableItemPrice to be called once with the gateway IDs applied");

        TransactionStoreMock.Verify(
            x => x.CommitAsync(
                It.Is<TransactionalWriteBatch>(batch =>
                    batch.Operations.Count == 3 &&
                    !string.IsNullOrEmpty(batch.IdempotencyKey)),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected CommitAsync to be called once with a batch of 3 operations and a valid idempotency key");

        ProductCreationWritePortMock.VerifyNoOtherCalls();
        ProductWritePortMock.VerifyNoOtherCalls();
        SellableItemWritePortMock.VerifyNoOtherCalls();
        SellableItemPriceWritePortMock.VerifyNoOtherCalls();
        TransactionStoreMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return success with product ID when gateway and commit succeed")]
    public async Task ShouldReturnProductIdWhenGatewayAndCommitSucceed()
    {
        // Given: a valid command, gateway and transactional commit both succeed
        CreateProductCommand command = BuildValidCommand();
        ProductGatewayCreatedResponseModel gatewayResponse = BuildGatewaySuccessResponse();

        ProductCreationWritePortMock
            .Setup(x => x.CreateProductInGatewayAsync(
                It.IsAny<ProductEntity>(),
                It.IsAny<SellableItemPriceEntity>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(gatewayResponse));

        TransactionStoreMock
            .Setup(x => x.CommitAsync(
                It.IsAny<TransactionalWriteBatch>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Unit.Value));

        // When: the handler processes the command
        Result<string> result = await Handler.Handle(command, CancellationToken.None);

        // Then: result should be success containing a non-empty Guid-format product ID
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value, Is.Not.Null.And.Not.Empty,
            $"Expected a non-empty ProductID but got: '{result.Value}'");
        Assert.That(Guid.TryParse(result.Value, out _), Is.True,
            $"Expected ProductID to be a valid Guid but got: '{result.Value}'");

        ProductCreationWritePortMock.Verify(
            x => x.CreateProductInGatewayAsync(
                It.Is<ProductEntity>(e =>
                    e.Name == command.Name &&
                    e.Description == command.Description &&
                    e.CreatedBy == TestUserId &&
                    !string.IsNullOrEmpty(e.ProductID)),
                It.Is<SellableItemPriceEntity>(p =>
                    p.Amount.Amount == command.Price &&
                    p.Amount.CurrencyCode == command.CurrencyCode),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected CreateProductInGatewayAsync called once with entities matching the command");

        ProductWritePortMock.Verify(
            x => x.CreateProduct(It.Is<ProductEntity>(e =>
                e.Name == command.Name &&
                e.CreatedBy == TestUserId)),
            Times.Once,
            "Expected CreateProduct called once with entity matching command and set CreatedBy");

        SellableItemWritePortMock.Verify(
            x => x.CreateSellableItem(It.Is<SellableItemEntity>(e =>
                e.Name == command.Name &&
                e.CreatedBy == TestUserId &&
                e.IsActive)),
            Times.Once,
            "Expected CreateSellableItem called once with active entity matching command");

        SellableItemPriceWritePortMock.Verify(
            x => x.CreateSellableItemPrice(
                It.Is<SellableItemPriceEntity>(p =>
                    p.ExternalProductID == gatewayResponse.ProductGatewayID &&
                    p.ExternalPriceID == gatewayResponse.ProductGatewayPriceID &&
                    p.CreatedBy == TestUserId)),
            Times.Once,
            "Expected CreateSellableItemPrice called once with gateway IDs and correct CreatedBy");

        TransactionStoreMock.Verify(
            x => x.CommitAsync(
                It.Is<TransactionalWriteBatch>(batch =>
                    batch.Operations.Count == 3 &&
                    !string.IsNullOrEmpty(batch.IdempotencyKey)),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected CommitAsync called once with a batch containing 3 operations and a non-empty idempotency key");

        ProductCreationWritePortMock.VerifyNoOtherCalls();
        ProductWritePortMock.VerifyNoOtherCalls();
        SellableItemWritePortMock.VerifyNoOtherCalls();
        SellableItemPriceWritePortMock.VerifyNoOtherCalls();
        TransactionStoreMock.VerifyNoOtherCalls();
    }
}
