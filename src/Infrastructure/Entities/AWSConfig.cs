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
    /// Secret used to encrypt and authenticate password reset tokens exchanged with the frontend.
    /// </summary>
    public string PasswordResetTokenSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the index on the Subscription table used for querying subscriptions by user ID.
    /// This property is used to specify the indexed attribute that allows efficient lookups for user-specific subscriptions in the database.
    /// </summary>
    [Required]
    public string SubscriptionUserIdIndex { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the DynamoDB index used for querying recordings by tier.
    /// This property is required for operations that involve filtering or retrieving recordings
    /// based on their assigned tier in the AWS DynamoDB infrastructure.
    /// </summary>
    [Required]
    public string RecordingsTierIndex { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the namespace used to organize and resolve AWS Systems Manager Parameter Store settings.
    /// This property is crucial for defining the hierarchical path where configuration parameters are stored and retrieved.
    /// </summary>
    [Required]
    public string SettingsNameSpace { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the private S3 bucket that stores recording video files.
    /// </summary>
    [Required]
    public string RecordingsBucketName { get; set; } = string.Empty;

    [Required]
    public string Environment { get; set; } = string.Empty;

}
