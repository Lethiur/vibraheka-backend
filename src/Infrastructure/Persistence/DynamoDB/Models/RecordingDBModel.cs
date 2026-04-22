using Amazon.DynamoDBv2.DataModel;
using VibraHeka.Domain.Recordings.Entities;
using VibraHeka.Domain.Recordings.Enums;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Converters;

namespace VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

[DynamoDBTable("TABLE_RECORDINGS")]
public class RecordingDBModel : BaseAuditableDBModel
{
    [DynamoDBHashKey]
    public string Id { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string Name { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string Description { get; set; } = string.Empty;

    [DynamoDBProperty(typeof(EnumStringConverter<RecordingType>))]
    public RecordingType Type { get; set; }

    [DynamoDBProperty]
    public string StorageKey { get; set; } = string.Empty;
    
    [DynamoDBProperty(typeof(EnumStringConverter<RecordingState>))]
    public RecordingState State { get; set; } = RecordingState.Active; 

    public static RecordingDBModel FromDomain(RecordingEntity entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description,
        Type = entity.Type,
        StorageKey = entity.StorageKey,
        Created = entity.Created,
        CreatedBy = entity.CreatedBy,
        LastModified = entity.LastModified,
        LastModifiedBy = entity.LastModifiedBy
    };
}
