declare namespace NodeJS {
    interface ProcessEnv {
        TEMPLATE_BUCKET: string;
        SES_FROM_EMAIL: string;
        SES_CONFIG_SET: string;
        SSM_TEMPLATE_NAME_PARAM?: string;
        SSM_VERIFICATION_TEMPLATE_NAME_PARAM?: string;
        SSM_PASSWORD_RESET_TEMPLATE_NAME_PARAM?: string;
        PASSWORD_RESET_TOKEN_SECRET?: string;
        PASSWORD_RESET_FRONTEND_URL?: string;
        PASSWORD_RESET_TOKEN_TTL_MINUTES?: string;
        KEY_ALIAS: string;
        KEY_ARN: string;
    }
}
