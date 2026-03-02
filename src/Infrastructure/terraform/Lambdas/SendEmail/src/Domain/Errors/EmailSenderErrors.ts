/**
 * Domain-level errors for the SendEmail lambda flow.
 */
enum EmailSenderErrors {
    /** Event payload is malformed or missing required structure. */
    INVALID_EVENT = "INVALID_EVENT",
    /** Required environment configuration is invalid. */
    INVALID_ENVIRONMENT = "INVALID_ENVIRONMENT",
    /** Recipient cannot be resolved from event payload. */
    MISSING_RECIPIENT = "MISSING_RECIPIENT",
    /** Encrypted Cognito code is missing in event payload. */
    MISSING_CODE = "MISSING_CODE",
    /** Cognito trigger source is not supported by this flow. */
    UNSUPPORTED_TRIGGER_SOURCE = "UNSUPPORTED_TRIGGER_SOURCE",
    /** KMS decryption of Cognito code failed. */
    DECRYPTION_FAILED = "DECRYPTION_FAILED",
    /** Template name/content could not be resolved from SSM/S3. */
    TEMPLATE_RESOLUTION_FAILED = "TEMPLATE_RESOLUTION_FAILED",
    /** Placeholder rendering failed. */
    TEMPLATE_RENDER_FAILED = "TEMPLATE_RENDER_FAILED",
    /** Building token or reset link failed. */
    TOKEN_BUILD_FAILED = "TOKEN_BUILD_FAILED",
    /** SES email send operation failed. */
    EMAIL_DELIVERY_FAILED = "EMAIL_DELIVERY_FAILED"
}

export default EmailSenderErrors;
