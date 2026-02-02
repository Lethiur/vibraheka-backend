/**
 * Environment variables required by the Lambda function
 */
export interface EnvironmentVariables {
    TEMPLATE_BUCKET: string;
    SES_FROM_EMAIL: string;
    SES_CONFIG_SET: string;
    SSM_TEMPLATE_NAME_PARAM: string;
    KEY_ALIAS: string;
    KEY_ARN: string;
}