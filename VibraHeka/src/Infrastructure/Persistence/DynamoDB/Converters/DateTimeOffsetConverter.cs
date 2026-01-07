using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace VibraHeka.Infrastructure.Persistence.DynamoDB.Converters;

public class DateTimeOffsetConverter : IPropertyConverter
{
    public DynamoDBEntry ToEntry(object value)
    {
        if (value is DateTimeOffset dto)
            return dto.ToString("O"); // Formato ISO 8601
        return new DynamoDBNull();
    }

    public object FromEntry(DynamoDBEntry entry)
    {
        if (entry is Primitive primitive && DateTimeOffset.TryParse(primitive.Value as string, out var dto))
            return dto;
        return default(DateTimeOffset);
    }
}
