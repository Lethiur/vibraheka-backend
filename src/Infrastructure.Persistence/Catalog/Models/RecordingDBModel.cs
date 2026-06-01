using Amazon.DynamoDBv2.DataModel;
using VibraHeka.Domain.Recordings.Enums;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Converters;

namespace Infrastructure.Persistence.Catalog.Models;

[DynamoDBTable("Recordings-Records")]
public class RecordingDBModel : ProductDBModel
{
    [DynamoDBHashKey]
    public string Id
    {
        get => ID;
        set => ID = value;
    }

    [DynamoDBProperty(typeof(EnumStringConverter<RecordingTier>))]
    public RecordingTier Tier { get; set; }

    [DynamoDBProperty(typeof(EnumStringConverter<RecordingType>))]
    public RecordingType RecordingType { get; set; }

}
