namespace VibraHeka.Application.Commerce.Commands.CreateOrder;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.dto).NotNull();

        RuleFor(x => x.dto.IdempotencyKey)
            .NotEmpty()
            .WithMessage("IdempotencyKey is required.");

        RuleFor(x => x.dto.OrderLines)
            .NotEmpty()
            .WithMessage("Order must contain at least one order line.");

        RuleForEach(x => x.dto.OrderLines).ChildRules(line =>
        {
            line.RuleFor(l => l.SellableItemID)
                .NotEmpty()
                .WithMessage("SellableItemID is required for each order line.");

            line.RuleFor(l => l.SellableItemPriceID)
                .NotEmpty()
                .WithMessage("SellableItemPriceID is required for each order line.");

            line.RuleFor(l => l.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than zero for each order line.");
        });
    }
}

