using System.ComponentModel;
using Amazon.DynamoDBv2.DocumentModel;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Converters;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.DynamoDB.Converters.EnumStringConverterTest;

[TestFixture]
public class FromEntityTest
{
    private EnumStringConverter<UserRole> _converter;

    [SetUp]
    public void SetUp()
    {
        _converter = new EnumStringConverter<UserRole>();
    }
    
    [Test]
    [DisplayName("Should convert valid string entry back to enum")]
    public void ShouldConvertValidStringEntryBackToEnum()
    {
        // Given: A DynamoDB primitive with a valid enum string
        DynamoDBEntry entry = new Primitive("Therapist");

        // When: Converting back to enum
        object result = _converter.FromEntry(entry);

        // Then: It should return the correct enum value
        Assert.That(result, Is.InstanceOf<UserRole>());
        Assert.That(result, Is.EqualTo(UserRole.Therapist));
    }

    [Test]
    [DisplayName("Should return DynamoDBNull when entry is not a primitive")]
    public void ShouldReturnDynamoDBNullWhenEntryIsNotAPrimitive()
    {
        // Given: A null entry or a non-primitive entry
        DynamoDBEntry nullEntry = null!;
        DynamoDBEntry complexEntry = new DynamoDBList();

        // When: Converting back
        object nullResult = _converter.FromEntry(nullEntry!);
        object complexResult = _converter.FromEntry(complexEntry);

        // Then: It should return DynamoDBNull
        Assert.That(nullResult, Is.InstanceOf<DynamoDBNull>());
        Assert.That(complexResult, Is.InstanceOf<DynamoDBNull>());
    }

    [Test]
    [DisplayName("Should return DynamoDBNull when string does not match enum")]
    public void ShouldReturnDynamoDBNullWhenStringDoesNotMatchEnum()
    {
        // Given: A string that is not part of the enum
        DynamoDBEntry entry = new Primitive("NonExistentRole");

        // When: Converting back
        object result = _converter.FromEntry(entry);

        // Then: It should return DynamoDBNull
        Assert.That(result, Is.InstanceOf<DynamoDBNull>());
    }

    [Test]
    [DisplayName("Should return DynamoDBNull when string is empty or whitespace")]
    [TestCase("")]
    [TestCase(" ")]
    public void ShouldReturnDynamoDBNullWhenStringIsEmptyOrWhitespace(string value)
    {
        // Given: An empty or whitespace string primitive
        DynamoDBEntry entry = new Primitive(value);

        // When: Converting back
        object result = _converter.FromEntry(entry);

        // Then: It should return DynamoDBNull
        Assert.That(result, Is.InstanceOf<DynamoDBNull>());
    }
}
