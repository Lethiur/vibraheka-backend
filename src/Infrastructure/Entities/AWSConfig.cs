using System.ComponentModel.DataAnnotations;

namespace VibraHeka.Infrastructure.Entities;

/// <summary>
/// Represents the configuration options for interacting with AWS services.
/// </summary>
public class AWSConfig
{
    /// <summary>
    /// Gets or sets the name of the S3 bucket that stores email templates.
    /// This property is used to define the bucket location for managing and retrieving email template files in the AWS infrastructure.
    /// </summary>
    [Required]
    public string EmailTemplatesBucketName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the DynamoDB table that stores email templates.
    /// This property defines the table location for managing and retrieving email template data within the AWS infrastructure.
    /// </summary>
    [Required]
    public string EmailTemplatesTable { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the DynamoDB table that stores user information.
    /// This property is used to define the database table location for managing and accessing user-related data in the AWS infrastructure.
    /// </summary>
    [Required]
    public string UsersTable { get; set; } = string.Empty;

    #if DEBUG
    /// <summary>
    /// Gets or sets the name of the DynamoDB table that stores code-related data.
    /// This property is used to define the table location for managing and accessing code entities within the AWS infrastructure.
    /// </summary>
    [Required]
    public string CodesTable { get; set; } = string.Empty;
    
    #endif

    /// <summary>
    /// Gets or sets the unique identifier for the AWS Cognito client application.
    /// This property is used to identify and authenticate the client application
    /// within the Cognito user pool for secure access and operations.
    /// </summary>
    [Required]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the Cognito User Pool.
    /// This property is used to specify the unique identifier of the AWS Cognito User Pool
    /// for managing user authentication and directory services.
    /// </summary>
    [Required]
    public string UserPoolId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the AWS region for service interactions.
    /// This property specifies the geographical region where AWS resources are deployed
    /// and determines the endpoint for connecting to AWS services.
    /// </summary>
    [Required]
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the profile name used for AWS service configurations.
    /// This property specifies the AWS credentials profile to be used
    /// when interacting with AWS resources in the application.
    /// </summary>
    [Required]
    public string Profile { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the DynamoDB table that stores action log entries.
    /// This property is used to define the table location for tracking and managing user or system activities within the AWS infrastructure.
    /// </summary>
    [Required]
    public string ActionLogTable { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the DynamoDB table that stores subscription data.
    /// This property specifies the table location used for managing subscription-related records in the AWS infrastructure.
    /// </summary>
    [Required]
    public string SubscriptionTable { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the index on the Subscription table used for querying subscriptions by user ID.
    /// This property is utilized to specify the indexed attribute that allows efficient lookups for user-specific subscriptions in the database.
    /// </summary>
    [Required]
    public string SubscriptionUserIdIndex { get; set; } = string.Empty;
}
