/**
 * Strongly typed environment configuration for the SendEmail lambda.
 */
export interface EnvironmentVariables {
    /** S3 bucket where template folders are stored. */
    TEMPLATE_BUCKET: string;
    /** SES source email address. */
    SES_FROM_EMAIL: string;
    /** SES configuration set name. */
    SES_CONFIG_SET: string;
    /** SSM parameter containing verification template folder/id. */
    SSM_VERIFICATION_TEMPLATE_NAME_PARAM: string;
    /** SSM parameter containing password-reset template folder/id. */
    SSM_PASSWORD_RESET_TEMPLATE_NAME_PARAM: string;
    /** Shared secret used to derive reset token encryption key. */
    PASSWORD_RESET_TOKEN_SECRET: string;
    /** Frontend route used by reset emails. */
    PASSWORD_RESET_FRONTEND_URL: string;
    /** Reset token TTL in minutes. */
    PASSWORD_RESET_TOKEN_TTL_MINUTES: number;
    /** KMS alias used by AWS Encryption SDK. */
    KEY_ALIAS: string;
    /** KMS key ARN used to decrypt Cognito codes. */
    KEY_ARN: string;
}
