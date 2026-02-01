import {EnvironmentVariables} from "../Interfaces/IEnvironmentVariables";

/**
 * Validates that all required environment variables are present
 * @throws Error if any required environment variable is missing
 */
export default function validateEnvironment(): EnvironmentVariables {
    const requiredVars = [
        'TEMPLATE_BUCKET',
        'SES_FROM_EMAIL',
        'SES_CONFIG_SET',
        'SSM_TEMPLATE_NAME_PARAM',
    ] as const;

    for (const varName of requiredVars) {
        if (!process.env[varName]) {
            throw new Error(`Missing required environment variable: ${varName}`);
        }
    }
    
    return {
        TEMPLATE_BUCKET: process.env.TEMPLATE_BUCKET!,
        SES_FROM_EMAIL: process.env.SES_FROM_EMAIL!,
        SES_CONFIG_SET: process.env.SES_CONFIG_SET!,
        SSM_TEMPLATE_NAME_PARAM: process.env.SSM_TEMPLATE_NAME_PARAM!,
    };
}
