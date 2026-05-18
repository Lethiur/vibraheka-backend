using System.Globalization;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using NMoneys;

namespace Infrastructure.Persistence.Converters;

public class MoneyConverter : IPropertyConverter
{
    private const string Separator = "|";
    public DynamoDBEntry ToEntry(object value)
    {
        if (value is not Money money)
        {
            return new DynamoDBNull();
        }

        var amount = money.Amount.ToString(CultureInfo.InvariantCulture);
        var currency = money.CurrencyCode.ToString();

        return new Primitive($"{amount}{Separator}{currency}");
    }

    public object FromEntry(DynamoDBEntry entry)
    {
        if (entry is not Primitive primitive)
        {
            return Money.Zero();
        }

        var raw = primitive.Value as string;

        if (string.IsNullOrWhiteSpace(raw))
        {
            return Money.Zero();
        }

        var parts = raw.Split(Separator);

        if (parts.Length != 2)
        {
            // Compatibilidad con datos antiguos tipo "9,9"
            if (decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var invariantAmount))
            {
                return new Money(invariantAmount);
            }

            if (decimal.TryParse(raw, NumberStyles.Number, new CultureInfo("es-ES"), out var spanishAmount))
            {
                return new Money(spanishAmount);
            }

            return Money.Zero();
        }

        var amount = decimal.Parse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture);
        var currency = parts[1];

        return new Money(amount, currency);
    }
}
