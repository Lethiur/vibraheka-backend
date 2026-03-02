import {EnvironmentVariables} from "../Interfaces/IEnvironmentVariables";

/**
 * Validates and returns environment variables required by the send-email lambda.
 *
 * @returns Parsed and validated environment configuration.
 * @throws Error when any required variable is missing or invalid.
 */
export default function validateEnvironment(): EnvironmentVariables {
    const verificationTemplateParam =
        process.env.SSM_VERIFICATION_TEMPLATE_NAME_PARAM ?? process.env.SSM_TEMPLATE_NAME_PARAM;

    const passwordResetTemplateParam =
        process.env.SSM_PASSWORD_RESET_TEMPLATE_NAME_PARAM ?? verificationTemplateParam;

    const requiredVars: Array<{ name: string; value: string | undefined }> = [
        {name: "TEMPLATE_BUCKET", value: process.env.TEMPLATE_BUCKET},
        {name: "SES_FROM_EMAIL", value: process.env.SES_FROM_EMAIL},
        {name: "SES_CONFIG_SET", value: process.env.SES_CONFIG_SET},
        {name: "SSM_VERIFICATION_TEMPLATE_NAME_PARAM", value: verificationTemplateParam},
        {name: "SSM_PASSWORD_RESET_TEMPLATE_NAME_PARAM", value: passwordResetTemplateParam},
        {name: "PASSWORD_RESET_TOKEN_SECRET", value: process.env.PASSWORD_RESET_TOKEN_SECRET},
        {name: "KEY_ALIAS", value: process.env.KEY_ALIAS},
        {name: "KEY_ARN", value: process.env.KEY_ARN}
    ];

    for (const variable of requiredVars) {
        if (!variable.value) {
            throw new Error(`Missing required environment variable: ${variable.name}`);
        }
    }

    const ttlMinutesRaw = process.env.PASSWORD_RESET_TOKEN_TTL_MINUTES ?? "15";
    const ttlMinutes = Number.parseInt(ttlMinutesRaw, 10);
    if (Number.isNaN(ttlMinutes) || ttlMinutes <= 0) {
        throw new Error("Missing or invalid environment variable: PASSWORD_RESET_TOKEN_TTL_MINUTES");
    }

    return {
        TEMPLATE_BUCKET: process.env.TEMPLATE_BUCKET!,
        SES_FROM_EMAIL: process.env.SES_FROM_EMAIL!,
        SES_CONFIG_SET: process.env.SES_CONFIG_SET!,
        SSM_VERIFICATION_TEMPLATE_NAME_PARAM: verificationTemplateParam!,
        SSM_PASSWORD_RESET_TEMPLATE_NAME_PARAM: passwordResetTemplateParam!,
        PASSWORD_RESET_TOKEN_SECRET: process.env.PASSWORD_RESET_TOKEN_SECRET!,
        PASSWORD_RESET_FRONTEND_URL: process.env.PASSWORD_RESET_FRONTEND_URL ?? "",
        PASSWORD_RESET_TOKEN_TTL_MINUTES: ttlMinutes,
        KEY_ALIAS: process.env.KEY_ALIAS!,
        KEY_ARN: process.env.KEY_ARN!
    };
}
