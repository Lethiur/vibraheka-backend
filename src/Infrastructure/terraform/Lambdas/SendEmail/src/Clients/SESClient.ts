import {SESClient, SendEmailCommand} from "@aws-sdk/client-ses";

const DEFAULT_FROM_EMAIL = "no-reply@vibraheka.com";

/**
 * Performs a basic email format validation.
 *
 * @param value Raw email value.
 * @returns True when value matches a minimal email pattern.
 */
function isValidEmailAddress(value: string): boolean {
    const normalized = value.trim();
    if (normalized.length === 0) {
        return false;
    }

    return /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(normalized);
}

/**
 * Wrapper over AWS SES client used to send HTML emails.
 */
export default class SESClientWrapper {
    constructor(private readonly sesClient: SESClient = new SESClient()) {}

    /**
     * Sends one email using SES.
     *
     * @param recipient Destination email address.
     * @param subject Email subject.
     * @param htmlBody Rendered HTML body.
     * @param fromEmail Configured sender email.
     * @param configSetName SES configuration set.
     * @returns Promise resolved when SES accepts the email request.
     */
    public async sendEmail(
        recipient: string,
        subject: string,
        htmlBody: string,
        fromEmail: string,
        configSetName: string
    ): Promise<void> {
        const normalizedRecipient = recipient.trim();
        if (!isValidEmailAddress(normalizedRecipient)) {
            throw new Error(`Invalid recipient email address: '${recipient}'`);
        }

        const normalizedFromEmail = fromEmail.trim();
        const sourceAddress = isValidEmailAddress(normalizedFromEmail)
            ? normalizedFromEmail
            : DEFAULT_FROM_EMAIL;

        if (sourceAddress !== normalizedFromEmail) {
            console.warn("SES_FROM_EMAIL is invalid. Falling back to default sender address.", {
                providedFromEmail: fromEmail,
                fallbackFromEmail: sourceAddress
            });
        }

        await this.sesClient.send(
            new SendEmailCommand({
                Source: sourceAddress,
                Destination: {
                    ToAddresses: [normalizedRecipient]
                },
                Message: {
                    Subject: {
                        Data: subject,
                        Charset: "UTF-8"
                    },
                    Body: {
                        Html: {
                            Data: htmlBody,
                            Charset: "UTF-8"
                        }
                    }
                },
                ConfigurationSetName: configSetName
            })
        );
    }
}
