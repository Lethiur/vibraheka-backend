using Amazon.DynamoDBv2.Model;

namespace Infrastructure.AWS.DynamoDB;

public class DynamoExpression
{
    public string Expression { get; set; } = "";
    public Dictionary<string, string> AttributeNames { get; set; } = new();
    public Dictionary<string, AttributeValue> AttributeValues { get; set; } = new();
}
