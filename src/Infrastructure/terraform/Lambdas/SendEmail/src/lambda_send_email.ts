import {
    CustomEmailSenderTriggerEvent, CustomEmailSenderTriggerHandler,
    CustomMessageTriggerEvent,
    CustomMessageTriggerHandler,
} from "aws-lambda";
import {
    buildClient,
    CommitmentPolicy,
    KmsKeyringNode,
} from '@aws-crypto/client-node';
import SSMClientWrapper from "./Clients/SSMClient";
import S3ClientWrapper from "./Clients/S3Client";
import validateEnvironment from "./Validators/EnvironmentValidator";
import SESClientWrapper from "./Clients/SESClient";
import {toByteArray} from 'base64-js';
const {decrypt} = buildClient(
    CommitmentPolicy.REQUIRE_ENCRYPT_ALLOW_DECRYPT
);

const sesClient = new SESClientWrapper();
const s3Client = new S3ClientWrapper();
const ssmClient = new SSMClientWrapper();

/**
 * Replaces template placeholders with actual values
 * Placeholders should be in the format {{variableName}}
 * @param template - The HTML template string with placeholders
 * @param data - Object containing key-value pairs for replacement
 * @returns The processed HTML with all placeholders replaced
 */
function processTemplate(template: string, data: Record<string, string | number>): string {
    let processedHtml = template;

    // Iterate through each key-value pair and replace {{key}} with value
    Object.entries(data).forEach(([key, value]) => {
        const regex = new RegExp(`{{${key}}}`, 'g');
        processedHtml = processedHtml.replace(regex, String(value));
    });

    return processedHtml;
}


export const handler: CustomEmailSenderTriggerHandler = async (event: CustomEmailSenderTriggerEvent) => {
    try {
        // Step 1: Validate all required environment variables are present
        const env = validateEnvironment();
        console.log(env)
        const recipient: string | null = event.userName;
        if (recipient == null) {
            throw new Error("El usuario es una puta mierda")
        }
        
        if (event.request.code == null) {
            throw new Error("El codigo es una puta mierda")
        }
        
        console.log(`Processing email request for ${recipient} recipient(s)`);

        // Step 2: Retrieve the template filename from SSM Parameter Store
        const templateFileName : string = await ssmClient.getParameter(env.SSM_TEMPLATE_NAME_PARAM);
        console.log(`Retrieved template name from SSM: ${templateFileName}/template.json`);

        // Step 3: Download the HTML template from S3
        const templateHtml = await s3Client.getFileContents(`${templateFileName}/template.json`, env.TEMPLATE_BUCKET);
        console.log(`Downloaded template from S3: ${env.TEMPLATE_BUCKET}/${templateFileName}`);

        const generatorKeyId: string = env.KEY_ALIAS;
        const keyIds: string[] = [env.KEY_ARN];
        const keyring = new KmsKeyringNode({generatorKeyId, keyIds});
        
        // Step 4: Process the template by replacing all {{variable}} placeholders
        // Decrypt the provided code using the AWS Encryption SDK
        const {plaintext} = await decrypt(
            keyring,
            toByteArray(event.request.code!)
        );
        const plainTextCode : string = plaintext.toString();
        const processedHtml = processTemplate(templateHtml, {"code": plainTextCode, "username": event.request.userAttributes.USER_NAME});

        // Step 5: Send emails to all recipients in parallel
        const subject = "Tu codigo de verificacion";

        await sesClient.sendEmail(recipient!, subject, processedHtml, env.SES_FROM_EMAIL, env.SES_CONFIG_SET);

        console.log(`Successfully sent email to ${recipient}`);

        return {
            statusCode: 200,
            body: JSON.stringify({
                message: `Emails sent successfully to ${recipient} recipient(s)`,
                recipients: recipient,
            }),
        };
    } catch (error) {
        // Log the error with full details for debugging
        console.error('Error sending email:', error);

        // Return a sanitized error response
        const errorMessage = error instanceof Error ? error.message : 'Unknown error occurred';

        return {
            statusCode: 500,
            body: JSON.stringify({
                message: 'Error sending email',
                error: errorMessage,
            }),
        };
    }
};
