using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace VibraHeka.Infrastructure.Persistence.DynamoDB.Converters;

/// <summary>
/// Provides a mechanism to convert enum values to and from their string representations
/// for storage in Amazon DynamoDB. This class implements the IPropertyConverter interface
/// and is generic to handle any enum type.
/// </summary>
/// <typeparam name="T">The enum type to be converted. Must be a struct.</typeparam>
public class EnumStringConverter<T> : IPropertyConverter where T : struct
{
    /// <summary>
    /// Converts an enum value to its corresponding DynamoDBEntry representation.
    /// This method is part of the implementation for storing enum values as strings in Amazon DynamoDB.
    /// </summary>
    /// <param name="value">The enum value to be converted. Must be an enum of type <typeparamref name="T"/>.</param>
    /// <returns>
    /// A <see cref="DynamoDBEntry"/>, specifically a <see cref="Primitive"/>, that
    /// represents the string version of the enum value.
    /// </returns>
    public DynamoDBEntry ToEntry(object value)
    {
        return new Primitive(value.ToString());
    }

    /// <summary>
    /// Converts a DynamoDBEntry back to its corresponding enum value.
    /// This method is part of the implementation for retrieving stored enum values from Amazon DynamoDB.
    /// </summary>
    /// <param name="entry">The <see cref="DynamoDBEntry"/> to be converted to an enum value.
    /// Must be a <see cref="Primitive"/> containing a valid string representation of the enum value.</param>
    /// <returns>
    /// An object representing the enum value of type <typeparamref name="T"/> if the conversion is successful.
    /// Returns a <see cref="DynamoDBNull"/> if the entry is null, not a <see cref="Primitive"/>,
    /// or the conversion fails.
    /// </returns>
    public object FromEntry(DynamoDBEntry entry)
    {
        if (entry is not Primitive primitive || string.IsNullOrWhiteSpace(primitive)) 
            return new DynamoDBNull();

        return Enum.TryParse(primitive, out T result) ? result : new DynamoDBNull();
    }
}
