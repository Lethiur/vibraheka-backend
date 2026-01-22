using Amazon.DynamoDBv2.DataModel;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Converters;

namespace VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

public class BaseAuditableDBModel
{
     
    [DynamoDBProperty(typeof(DateTimeOffsetConverter))]
    public DateTimeOffset Created { get; set; }

    [DynamoDBProperty] 
    public string? CreatedBy { get; set; }

    [DynamoDBProperty(typeof(DateTimeOffsetConverter))] 
    public DateTimeOffset LastModified { get; set; }

    [DynamoDBProperty] 
    public string? LastModifiedBy { get; set; }
}
