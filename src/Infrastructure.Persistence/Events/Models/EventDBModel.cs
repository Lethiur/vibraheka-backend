using Amazon.DynamoDBv2.DataModel;
using Infrastructure.Persistence.Catalog.Models;
using VibraHeka.Domain.Events.Enums;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Converters;

namespace Infrastructure.Persistence.Events.Models;

[DynamoDBTable("Events-Records")]
public class EventDBModel : ProductDBModel
{
    [DynamoDBHashKey]
    public string EventID { get => ID; set => ID = value; }

    [DynamoDBGlobalSecondaryIndexHashKey("EventsByDate-Index")]
    [DynamoDBProperty(typeof(DateTimeOffsetConverter))]
    public DateTimeOffset EventDateUtc { get; set; }

    [DynamoDBProperty]
    public int Duration { get; set; } = 0;

    [DynamoDBProperty]
    public string EventPassword { get; set; } = string.Empty;

    [DynamoDBProperty]
    [DynamoDBGlobalSecondaryIndexRangeKey("EventsByDate-Index")]
    public string EventTimezone { get; set; } = string.Empty;

    [DynamoDBProperty(typeof(EnumStringConverter<EventStatus>))]
    public EventStatus Status { get; set; } = EventStatus.MissingLink;

    [DynamoDBProperty]
    public string EventLink { get; set; } = string.Empty;
}
