import {SESClient, SendEmailCommand} from '@aws-sdk/client-ses';

/**
 * A wrapper class for the AWS Simple Email Service (SES) client. It provides a convenient method to send emails using the SES API.
 */
export default class SESClientWrapper {
    
    constructor(private readonly sesClient: SESClient = new SESClient()) {}

    /**
     * Sends an email using the specified parameters.
     *
     * @param {string} recipient - The email address of the recipient.
     * @param {string} subject - The subject line of the email.
     * @param {string} htmlBody - The HTML content of the email body.
     * @param {string} fromEmail - The sender's email address.
     * @param {string} configSetName - The name of the configuration set to use for the email.
     * @return {Promise<void>} A promise that resolves when the email is sent successfully.
     */
    public async sendEmail(  recipient: string,
                             subject: string,
                             htmlBody: string,
                             fromEmail: string,
                             configSetName: string): Promise<void> {
        await this.sesClient.send(
            new SendEmailCommand({
                Source: "no-reply@vibraheka.com",
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
}