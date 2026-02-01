declare namespace NodeJS {
    interface ProcessEnv {
        TEMPLATE_BUCKET: string;
        SES_FROM_EMAIL: string;
        SES_CONFIG_SET: string;
        SSM_TEMPLATE_NAME_PARAM: string;
    }
}