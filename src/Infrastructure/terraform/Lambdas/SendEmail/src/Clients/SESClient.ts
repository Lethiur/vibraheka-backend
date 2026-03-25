import {Attachment, SESv2Client, SendEmailCommand, SendEmailCommandOutput} from "@aws-sdk/client-sesv2";
import {NotificationEmailAttachment} from "@Domain/Entities/NotificationEmailEvent";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";
import {err, ok, Result} from "neverthrow";

/**
 * Wrapper over AWS SES client used to send HTML emails.
 */
export default class SESClientWrapper {
    constructor(private readonly sesClient: SESv2Client = new SESv2Client()) {
    }

    /**
     * Sends one email using SES.
     *
     * @param recipient Destination email address.
     * @param subject Email subject.
     * @param htmlBody Rendered HTML body.
     * @param fromEmail Configured sender email.
     * @param configSetName SES configuration set.
     * @param attachments The list of attachment to send with the email
     * @returns Promise resolved when SES accepts the email request.
     */
    public async sendEmail(recipient: string, subject: string, htmlBody: string, fromEmail: string, configSetName: string,
                           attachments: NotificationEmailAttachment[] = []
    ): Promise<Result<void, EmailSenderErrors>> {
        const normalizedRecipient = recipient.trim();
        const normalizedFromEmail = fromEmail.trim();

        const emailAttachments: Attachment[] = await Promise.all(attachments.map(this.NotificationEmailAttachmentToSesAttachment));

        try {
            const result: SendEmailCommandOutput = await this.sesClient.send(
                new SendEmailCommand({
                    FromEmailAddress: normalizedFromEmail,
                    Destination: {
                        ToAddresses: [normalizedRecipient]
                    },
                    ConfigurationSetName: configSetName,
                    Content: {

                        Simple: {
                            Subject: {
                                Data: subject,
                                Charset: "UTF-8"
                            },
                            Body: {
                                Html: {
                                    Data: htmlBody,
                                }
                            },
                            Attachments: emailAttachments,
                        },

                    }
                })
            );    
            if (result.$metadata.httpStatusCode == 200) {
                return ok(undefined);
            }
            
            console.log("SES returned a code different than 200", result);
            return err(EmailSenderErrors.EMAIL_DELIVERY_FAILED);
        } catch (e) {
            console.error(`Failed to send email: ${(e as Error).message}`);
            return err(EmailSenderErrors.EMAIL_DELIVERY_FAILED);
        }
        
    }

    private async NotificationEmailAttachmentToSesAttachment(attachment: NotificationEmailAttachment): Promise<Attachment> {
        const response = await fetch(attachment.attachmentUrl);

        if (!response.ok) {
            throw new Error(`Download failed: ${response.status} ${response.statusText}`);
        }

        const arrayBuffer: ArrayBuffer = await response.arrayBuffer();
        const buffer: Buffer = Buffer.from(arrayBuffer);
        console.log("Attachment downloaded", {
            fileName: attachment.attachmentName,
            attachmentType: attachment.attachmentType,
            size: buffer.length,
            firstBytesHex: buffer.subarray(0, 8).toString("hex"),
        });

        return {
            RawContent: buffer,
            FileName: attachment.attachmentName,
            ContentType: attachment.attachmentType,
            ContentDisposition: 'ATTACHMENT',
            ContentTransferEncoding: "BASE64"
        };
    }
}
