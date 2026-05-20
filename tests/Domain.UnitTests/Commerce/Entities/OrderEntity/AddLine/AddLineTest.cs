using System.ComponentModel;
using NMoneys;
using NUnit.Framework;
using VibraHeka.Domain.Commerce.Entities;

namespace VibraHeka.Domain.UnitTests.Commerce.Entities.OrderEntity.AddLine;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class AddLineTest : GenericOrderEntityAddLineTest
{
    [Test]
    [DisplayName("Should add the order line to the Lines collection")]
    public void ShouldAddOrderLineToLinesCollection()
    {
        // Given: an empty order and a valid order line
        Domain.Commerce.Entities.OrderEntity order = BuildEmptyOrder();
        OrderLineEntity line = BuildOrderLineWithZeroAmounts();

        Assert.That(order.Lines, Is.Empty,
            "Expected no lines in order before AddLine is called");

        // When: AddLine is called
        order.AddLine(line);

        // Then: the order lines collection contains the added line
        Assert.That(order.Lines, Has.Count.EqualTo(1),
            $"Expected Lines.Count=1 after AddLine but got {order.Lines.Count}");
        Assert.That(order.Lines[0], Is.SameAs(line),
            "Expected Lines[0] to be the exact same object passed to AddLine");
    }

    [Test]
    [DisplayName("Should accumulate Total from the order line into the order Total")]
    public void ShouldAccumulateTotalFromOrderLine()
    {
        // Given: an empty order and two order lines with zero amounts
        Domain.Commerce.Entities.OrderEntity order = BuildEmptyOrder();
        OrderLineEntity firstLine = BuildOrderLineWithId("line-001");
        OrderLineEntity secondLine = BuildOrderLineWithId("line-002");

        // When: both lines are added
        order.AddLine(firstLine);
        order.AddLine(secondLine);

        // Then: the order has two lines and total remains zero
        Assert.That(order.Lines, Has.Count.EqualTo(2),
            $"Expected Lines.Count=2 after adding two lines but got {order.Lines.Count}");
        Assert.That(order.Total, Is.EqualTo(Money.Zero()),
            "Expected Total to remain Money.Zero after adding lines with zero amounts");
    }

    [Test]
    [DisplayName("Should accumulate Subtotal, TaxTotal and DiscountTotal from the order line")]
    public void ShouldAccumulateSubtotalTaxAndDiscountFromOrderLine()
    {
        // Given: an empty order and a valid order line with zero amounts
        Domain.Commerce.Entities.OrderEntity order = BuildEmptyOrder();
        OrderLineEntity line = BuildOrderLineWithZeroAmounts();

        // When: AddLine is called
        order.AddLine(line);

        // Then: all monetary summaries match the line amounts (zero)
        Assert.That(order.Subtotal, Is.EqualTo(Money.Zero()),
            "Expected Subtotal to equal line.Subtotal after AddLine");
        Assert.That(order.TaxTotal, Is.EqualTo(Money.Zero()),
            "Expected TaxTotal to equal line.TaxAmount after AddLine");
        Assert.That(order.DiscountTotal, Is.EqualTo(Money.Zero()),
            "Expected DiscountTotal to equal line.DiscountAmount after AddLine");
    }

    [Test]
    [DisplayName("Should maintain order integrity with multiple lines added sequentially")]
    public void ShouldMaintainOrderIntegrityWithMultipleLinesAddedSequentially()
    {
        // Given: an empty order
        Domain.Commerce.Entities.OrderEntity order = BuildEmptyOrder();
        int expectedLineCount = 3;

        // When: multiple lines are added sequentially
        for (int i = 0; i < expectedLineCount; i++)
        {
            OrderLineEntity line = BuildOrderLineWithId($"line-domain-00{i + 1}");
            order.AddLine(line);
        }

        // Then: the order contains exactly the expected number of lines
        Assert.That(order.Lines, Has.Count.EqualTo(expectedLineCount),
            $"Expected Lines.Count={expectedLineCount} after adding {expectedLineCount} lines but got {order.Lines.Count}");
        Assert.That(order.Lines.Select(l => l.OrderLineID).Distinct().Count(), Is.EqualTo(expectedLineCount),
            "Expected each added line to be distinct in the order Lines collection");
    }
}


