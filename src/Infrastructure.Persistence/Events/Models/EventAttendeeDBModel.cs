using Amazon.DynamoDBv2.DataModel;

namespace Infrastructure.Persistence.Events.Models;

[DynamoDBTable("Events-Attendees")]
public class EventAttendeeDBModel
{
    [DynamoDBHashKey]
    public String AttendeeID { get; set; } = string.Empty;

    [DynamoDBProperty]
    public String AttendeeName { get; set; } = string.Empty;

    [DynamoDBGlobalSecondaryIndexHashKey("UserID-Index")]
    [DynamoDBProperty]
    public String UserID { get; set; } = string.Empty;

    [DynamoDBProperty]
    public String Email { get; set; } = string.Empty;

    [DynamoDBGlobalSecondaryIndexHashKey("EventID-Index")]
    [DynamoDBProperty]
    public String EventID { get; set; } = string.Empty;

}
