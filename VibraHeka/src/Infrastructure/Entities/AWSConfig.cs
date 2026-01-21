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
    public string EmailTemplatesBucketName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the DynamoDB table that stores email templates.
    /// This property defines the table location for managing and retrieving email template data within the AWS infrastructure.
    /// </summary>
    public string EmailTemplatesTable { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the DynamoDB table that stores user information.
    /// This property is used to define the database table location for managing and accessing user-related data in the AWS infrastructure.
    /// </summary>
    public string UsersTable { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the DynamoDB table that stores code-related data.
    /// This property is used to define the table location for managing and accessing code entities within the AWS infrastructure.
    /// </summary>
    public string CodesTable { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier for the AWS Cognito client application.
    /// This property is used to identify and authenticate the client application
    /// within the Cognito user pool for secure access and operations.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the Cognito User Pool.
    /// This property is used to specify the unique identifier of the AWS Cognito User Pool
    /// for managing user authentication and directory services.
    /// </summary>
    public string UserPoolId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the AWS region for service interactions.
    /// This property specifies the geographical region where AWS resources are deployed
    /// and determines the endpoint for connecting to AWS services.
    /// </summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the profile name used for AWS service configurations.
    /// This property specifies the AWS credentials profile to be used
    /// when interacting with AWS resources in the application.
    /// </summary>
    public string Profile { get; set; } = string.Empty;


}
