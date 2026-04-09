using System.Globalization;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace VibraHeka.Infrastructure.Persistence.DynamoDB.Converters;

public class DateTimeOffsetConverter : IPropertyConverter
{
    public DynamoDBEntry ToEntry(object value)
    {
        // Si el valor es DateTimeOffset y no es la fecha mínima (evitamos basura en DB)
        if (value is DateTimeOffset dto && dto != DateTimeOffset.MinValue)
        {
            return new Primitive(dto.ToString("O", CultureInfo.InvariantCulture));
        }
        
        return new DynamoDBNull();
    }

    public object FromEntry(DynamoDBEntry entry)
    {
        Primitive? primitive = entry as Primitive;
        if (primitive == null || string.IsNullOrWhiteSpace(primitive.Value as string))
        {
            return DateTimeOffset.MinValue;
        }

        if (DateTimeOffset.TryParse(primitive.Value as string, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTimeOffset dto))
        {
            return dto;
        }

        return DateTimeOffset.MinValue;
    }
}
