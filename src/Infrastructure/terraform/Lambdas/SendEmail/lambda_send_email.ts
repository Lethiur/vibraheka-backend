import { SESClient, SendEmailCommand } from '@aws-sdk/client-ses';
import { S3Client, GetObjectCommand } from '@aws-sdk/client-s3';
import { SSMClient, GetParameterCommand } from '@aws-sdk/client-ssm';
import {
    CustomMessageTriggerEvent,
    CustomMessageTriggerHandler,
} from "aws-lambda";



// Initialize AWS SDK clients with default configuration
const sesClient = new SESClient({});
const s3Client = new S3Client({});
const ssmClient = new SSMClient({});

/**
 * Interface defining the structure of incoming email requests
 */
interface EmailEvent {
  /** Array of recipient email addresses */
  to: string[];
  /** Key-value pairs to replace in the email template (e.g., {{name}} -> "John") */
  templateData: Record<string, string | number>;
  /** Optional email subject line */
  subject?: string;
}

/**
 * Interface for the Lambda function response
 */
interface EmailResponse {
  statusCode: number;
  body: string;
}

/**
 * Environment variables required by the Lambda function
 */
interface EnvironmentVariables {
  TEMPLATE_BUCKET: string;
  SES_FROM_EMAIL: string;
  SES_CONFIG_SET: string;
  SSM_TEMPLATE_NAME_PARAM: string;
}

/**
 * Validates that all required environment variables are present
 * @throws Error if any required environment variable is missing
 */
function validateEnvironment(): EnvironmentVariables {
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

/**
 * Retrieves the email template filename from AWS Systems Manager Parameter Store
 * @param parameterName - The SSM parameter name containing the template filename
 * @returns The template filename stored in SSM
 * @throws Error if parameter is not found or value is empty
 */
async function getTemplateNameFromSSM(parameterName: string): Promise<string> {
  const response = await ssmClient.send(
    new GetParameterCommand({
      Name: parameterName,
      WithDecryption: false,
    })
  );

  const templateFileName = response.Parameter?.Value;

  if (!templateFileName) {
    throw new Error(`Template name not found in SSM parameter: ${parameterName}`);
  }

  return templateFileName;
}

/**
 * Downloads and retrieves the HTML email template from S3
 * @param bucketName - Name of the S3 bucket containing templates
 * @param templateKey - The S3 object key (filename) of the template
 * @returns The HTML template content as a string
 * @throws Error if template is not found or cannot be read
 */
async function getTemplateFromS3(bucketName: string, templateKey: string): Promise<string> {
  const response = await s3Client.send(
    new GetObjectCommand({
      Bucket: bucketName,
      Key: templateKey,
    })
  );

  if (!response.Body) {
    throw new Error(`Template not found in S3: ${bucketName}/${templateKey}`);
  }

  const templateHtml = await response.Body.transformToString('utf-8');

  if (!templateHtml) {
    throw new Error('Template content is empty');
  }
  console.log(templateHtml)
  return  templateHtml;
  
}

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

/**
 * Sends an email to a single recipient using AWS SES
 * @param recipient - Email address of the recipient
 * @param subject - Email subject line
 * @param htmlBody - HTML content of the email
 * @param fromEmail - Sender email address (must be verified in SES)
 * @param configSetName - SES configuration set for tracking
 * @throws Error if email sending fails
 */
async function sendEmail(
  recipient: string,
  subject: string,
  htmlBody: string,
  fromEmail: string,
  configSetName: string
): Promise<void> {
  await sesClient.send(
    new SendEmailCommand({
      Source: fromEmail,
      Destination: {
        ToAddresses: [recipient],
      },
      Message: {
        Subject: {
          Data: subject,
          Charset: 'UTF-8',
        },
        Body: {
          Html: {
            Data: htmlBody,
            Charset: 'UTF-8',
          },
        },
      },
      ConfigurationSetName: configSetName,
    })
  );
}


export const handler: CustomMessageTriggerHandler = async (event: CustomMessageTriggerEvent) => {
  try {
    // Step 1: Validate all required environment variables are present
    const env = validateEnvironment();
    console.log(event);
      const recipient : string | null = event.request.usernameParameter;
      if (recipient == null) {
          throw new Error("El usuario es una puta mierda")
      }

      console.log(`Processing email request for ${recipient} recipient(s)`);

    // Step 2: Retrieve the template filename from SSM Parameter Store
    const templateFileName = await getTemplateNameFromSSM(env.SSM_TEMPLATE_NAME_PARAM);
    console.log(`Retrieved template name from SSM: ${templateFileName}/template.json`);

    // Step 3: Download the HTML template from S3
    const templateHtml = await getTemplateFromS3(env.TEMPLATE_BUCKET, `${templateFileName}/template.json`);
    console.log(`Downloaded template from S3: ${env.TEMPLATE_BUCKET}/${templateFileName}`);

    // Step 4: Process the template by replacing all {{variable}} placeholders
    const processedHtml = processTemplate(templateHtml, {"code": event.request.codeParameter});

    // Step 5: Send emails to all recipients in parallel
    const defaultSubject = 'Email from VibraHeka';
    const subject = "Tu codigo de verificacion";
    
      await sendEmail(recipient!, subject, processedHtml, env.SES_FROM_EMAIL, env.SES_CONFIG_SET);

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
