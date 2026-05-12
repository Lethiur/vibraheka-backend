using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using NMoneys;

namespace Infrastructure.Persistence.Converters;

public class MoneyConverter : IPropertyConverter
{
    public DynamoDBEntry ToEntry(object value)
    {
        if (value is Money money)
        {
            return new Primitive(money.ToString());
        }
        return new DynamoDBNull();
    }

    public object FromEntry(DynamoDBEntry entry)
    {
        Primitive? primitive = entry as Primitive;
        if (primitive == null || string.IsNullOrWhiteSpace(primitive.Value as string))
        {
            return Money.Zero();
        }
        
        return Money.Parse(primitive.Value as string ?? "");
    }
}
