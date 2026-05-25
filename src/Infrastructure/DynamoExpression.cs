using Amazon.DynamoDBv2.Model;

namespace VibraHeka.Infrastructure;

public class DynamoExpression
{
    public string IndexName { get; set; } = "";

    /// <summary>
    /// Used as KeyConditionExpression in queries and UpdateExpression in updates.
    /// Must reference only key attributes when used in QueryIndexAsync.
    /// </summary>
    public string Expression { get; set; } = "";

    /// <summary>
    /// Optional non-key filter applied after the key condition in a Query.
    /// Set this for attributes that are NOT part of the index key schema.
    /// </summary>
    public string? FilterExpression { get; set; }

    public Dictionary<string, string> AttributeNames { get; set; } = new();
    public Dictionary<string, AttributeValue> AttributeValues { get; set; } = new();
}
