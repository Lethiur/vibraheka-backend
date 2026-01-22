using System.ComponentModel;
using Amazon.DynamoDBv2.DocumentModel;
using DateTimeOffsetConverter = VibraHeka.Infrastructure.Persistence.DynamoDB.Converters.DateTimeOffsetConverter;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.DynamoDB.Converters.DateTimeOffsetConverterTest;

public class FromEntityTest
{
    private DateTimeOffsetConverter Converter;

    [SetUp]
    public void SetUp()
    {
        Converter = new DateTimeOffsetConverter();
    }

    [Test]
    [DisplayName("Should convert valid ISO 8601 string entry to DateTimeOffset")]
    public void ShouldConvertValidIsoStringEntryToDateTimeOffset()
    {
        // Given: A valid ISO 8601 date string in a DynamoDB primitive
        const string dateString = "2024-03-20T15:30:00.0000000+01:00";
        DynamoDBEntry entry = new Primitive(dateString);

        // When: Converting back to object
        object result = Converter.FromEntry(entry);

        // Then: It should be a DateTimeOffset with the correct values
        Assert.That(result, Is.InstanceOf<DateTimeOffset>());
        DateTimeOffset dto = (DateTimeOffset)result;
        Assert.That(dto.Year, Is.EqualTo(2024));
        Assert.That(dto.Offset, Is.EqualTo(TimeSpan.FromHours(1)));
    }

    [Test]
    [DisplayName("Should handle Zulu time format correctly")]
    public void ShouldHandleZuluTimeFormatCorrectly()
    {
        // Given: A date string in UTC (Zulu) format
        string dateString = "2026-01-14T10:00:00.0000000Z";
        DynamoDBEntry entry = new Primitive(dateString);

        // When: Converting back
        object result = Converter.FromEntry(entry);

        // Then: The offset should be zero (UTC)
        DateTimeOffset dto = (DateTimeOffset)result;
        Assert.That(dto.Offset, Is.EqualTo(TimeSpan.Zero));
    }

    [Test]
    [DisplayName("Should return MinValue when entry is null or not a primitive")]
    public void ShouldReturnMinValueWhenEntryIsInvalid()
    {
        // Given: Invalid entries
        DynamoDBEntry nullEntry = null!;
        DynamoDBEntry listEntry = new DynamoDBList();

        // When: Converting
        object resultNull = Converter.FromEntry(nullEntry);
        object resultList = Converter.FromEntry(listEntry);

        // Then: Should return DateTimeOffset.MinValue
        Assert.That(resultNull, Is.EqualTo(DateTimeOffset.MinValue));
        Assert.That(resultList, Is.EqualTo(DateTimeOffset.MinValue));
    }

    [Test]
    [DisplayName("Should return MinValue when string is not a valid date")]
    public void ShouldReturnMinValueWhenStringIsInvalidDate()
    {
        // Given: A primitive with non-date text
        DynamoDBEntry entry = new Primitive("not-a-date");

        // When: Converting
        object result = Converter.FromEntry(entry);

        // Then: Should return DateTimeOffset.MinValue
        Assert.That(result, Is.EqualTo(DateTimeOffset.MinValue));
    }

    [Test]
    [DisplayName("Should return MinValue when string is empty or whitespace")]
    [TestCase("")]
    [TestCase("   ")]
    public void ShouldReturnMinValueWhenStringIsEmpty(string value)
    {
        // Given: An empty string entry
        DynamoDBEntry entry = new Primitive(value);

        // When: Converting
        object result = Converter.FromEntry(entry);

        // Then: Should return DateTimeOffset.MinValue
        Assert.That(result, Is.EqualTo(DateTimeOffset.MinValue));
    }
}
