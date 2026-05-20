using System.ComponentModel;
using FluentValidation.TestHelper;
using NUnit.Framework;
using VibraHeka.Application.Commerce.Commands.CreateOrder;
using VibraHeka.Application.Commerce.Models;

namespace VibraHeka.Application.UnitTests.Commerce.Commands.CreateOrder;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class CreateOrderCommandValidatorTest : GenericCreateOrderTest
{
    private CreateOrderCommandValidator Validator = default!;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        Validator = new CreateOrderCommandValidator();
    }

    #region IdempotencyKey Validation

    [Test]
    [DisplayName("Should fail validation when IdempotencyKey is empty")]
    public void ShouldFailValidationWhenIdempotencyKeyIsEmpty()
    {
        // Given: a command with an empty IdempotencyKey
        CreateOrderCommand command = new(new CreateOrderDTO
        {
            IdempotencyKey = string.Empty,
            OrderLines =
            [
                new CreateOrderLineDTO { SellableItemID = "item-1", SellableItemPriceID = "price-1", Quantity = 1 }
            ]
        });

        // When: validating the command
        TestValidationResult<CreateOrderCommand> result = Validator.TestValidate(command);

        // Then: should have a validation error for IdempotencyKey
        result.ShouldHaveValidationErrorFor(x => x.dto.IdempotencyKey);
    }

    #endregion

    #region OrderLines Validation

    [Test]
    [DisplayName("Should fail validation when OrderLines is empty")]
    public void ShouldFailValidationWhenOrderLinesIsEmpty()
    {
        // Given: a command with no order lines
        CreateOrderCommand command = new(new CreateOrderDTO
        {
            IdempotencyKey = FakeIdempotencyKey,
            OrderLines = []
        });

        // When: validating the command
        TestValidationResult<CreateOrderCommand> result = Validator.TestValidate(command);

        // Then: should have a validation error for OrderLines
        result.ShouldHaveValidationErrorFor(x => x.dto.OrderLines);
    }

    [Test]
    [DisplayName("Should fail validation when SellableItemID is empty in a line")]
    public void ShouldFailValidationWhenSellableItemIdIsEmptyInALine()
    {
        // Given: a command with a line that has an empty SellableItemID
        CreateOrderCommand command = new(new CreateOrderDTO
        {
            IdempotencyKey = FakeIdempotencyKey,
            OrderLines =
            [
                new CreateOrderLineDTO { SellableItemID = string.Empty, SellableItemPriceID = "price-1", Quantity = 1 }
            ]
        });

        // When: validating the command
        TestValidationResult<CreateOrderCommand> result = Validator.TestValidate(command);

        // Then: should have a validation error for SellableItemID in the line
        result.ShouldHaveValidationErrorFor("dto.OrderLines[0].SellableItemID");
    }

    [Test]
    [DisplayName("Should fail validation when SellableItemPriceID is empty in a line")]
    public void ShouldFailValidationWhenSellableItemPriceIdIsEmptyInALine()
    {
        // Given: a command with a line that has an empty SellableItemPriceID
        CreateOrderCommand command = new(new CreateOrderDTO
        {
            IdempotencyKey = FakeIdempotencyKey,
            OrderLines =
            [
                new CreateOrderLineDTO { SellableItemID = "item-1", SellableItemPriceID = string.Empty, Quantity = 1 }
            ]
        });

        // When: validating the command
        TestValidationResult<CreateOrderCommand> result = Validator.TestValidate(command);

        // Then: should have a validation error for SellableItemPriceID in the line
        result.ShouldHaveValidationErrorFor("dto.OrderLines[0].SellableItemPriceID");
    }

    [Test]
    [DisplayName("Should fail validation when Quantity is zero in a line")]
    public void ShouldFailValidationWhenQuantityIsZeroInALine()
    {
        // Given: a command with a line that has Quantity = 0
        CreateOrderCommand command = new(new CreateOrderDTO
        {
            IdempotencyKey = FakeIdempotencyKey,
            OrderLines =
            [
                new CreateOrderLineDTO { SellableItemID = "item-1", SellableItemPriceID = "price-1", Quantity = 0 }
            ]
        });

        // When: validating the command
        TestValidationResult<CreateOrderCommand> result = Validator.TestValidate(command);

        // Then: should have a validation error for Quantity in the line
        result.ShouldHaveValidationErrorFor("dto.OrderLines[0].Quantity");
    }

    [Test]
    [DisplayName("Should fail validation when Quantity is negative in a line")]
    public void ShouldFailValidationWhenQuantityIsNegativeInALine()
    {
        // Given: a command with a line that has Quantity = -1
        CreateOrderCommand command = new(new CreateOrderDTO
        {
            IdempotencyKey = FakeIdempotencyKey,
            OrderLines =
            [
                new CreateOrderLineDTO { SellableItemID = "item-1", SellableItemPriceID = "price-1", Quantity = -1 }
            ]
        });

        // When: validating the command
        TestValidationResult<CreateOrderCommand> result = Validator.TestValidate(command);

        // Then: should have a validation error for Quantity in the line
        result.ShouldHaveValidationErrorFor("dto.OrderLines[0].Quantity");
    }

    #endregion

    #region Happy Path

    [Test]
    [DisplayName("Should pass validation when all fields are valid")]
    public void ShouldPassValidationWhenAllFieldsAreValid()
    {
        // Given: a fully valid command
        CreateOrderCommand command = BuildValidCommand();

        // When: validating the command
        TestValidationResult<CreateOrderCommand> result = Validator.TestValidate(command);

        // Then: should not have any validation errors
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion
}

