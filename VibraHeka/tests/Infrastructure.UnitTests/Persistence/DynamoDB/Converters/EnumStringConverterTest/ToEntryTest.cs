using System.ComponentModel;
using Amazon.DynamoDBv2.DocumentModel;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Converters;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.DynamoDB.Converters.EnumStringConverterTest;

[TestFixture]
public class ToEntryTest
{
    private EnumStringConverter<UserRole> Converter;

    [SetUp]
    public void SetUp()
    {
        Converter = new EnumStringConverter<UserRole>();
    }
    
    [Test]
    [DisplayName("Should convert enum value to string primitive")]
    public void ShouldConvertEnumValueToStringPrimitive()
    {
        // Given: An enum value
        UserRole role = UserRole.Admin;

        // When: Converting to DynamoDBEntry
        DynamoDBEntry entry = Converter.ToEntry(role);

        // Then: It should be a Primitive containing the string representation
        Assert.That(entry, Is.InstanceOf<Primitive>());
        Assert.That(entry.AsString(), Is.EqualTo("Admin"));
    }

    [Test]
    [DisplayName("Should convert to dynamoDB null if the role is not present")]
    public void ShouldConvertEnumValueToNull()
    {
    
        // When: Converting to DynamoDBEntry
        DynamoDBEntry entry = Converter.ToEntry(null!);

        // Then: It should be a dynamo db null
        Assert.That(entry, Is.InstanceOf<DynamoDBNull>());
    }
    
    
}
