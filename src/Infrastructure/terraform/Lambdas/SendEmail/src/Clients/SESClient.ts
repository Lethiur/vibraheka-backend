import {Attachment, SESv2Client, SendEmailCommand, SendEmailCommandOutput} from "@aws-sdk/client-sesv2";
import {NotificationEmailAttachment} from "@Domain/Entities/NotificationEmailEvent";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";
import {err, ok, okAsync, Result, ResultAsync} from "neverthrow";

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
    public sendEmail(recipient: string, subject: string, htmlBody: string, fromEmail: string, configSetName: string,
                           attachments: NotificationEmailAttachment[] = []
    ): ResultAsync<void, EmailSenderErrors> {
        const normalizedRecipient = recipient.trim();
        const normalizedFromEmail = fromEmail.trim();

        return ResultAsync.fromPromise(Promise.all(attachments.map(this.NotificationEmailAttachmentToSesAttachment)), error => {
            console.log("Failed to convert attachments to SES format", error);
            throw new Error(EmailSenderErrors.ERROR_FETCHING_ATTACHMENT);
        }).map(results => results.filter(result => result.isOk()))
            .map(result => {
                return new SendEmailCommand({
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
                            Attachments: result.filter(result => result.isOk())
                                ?.map(res => res.value),
                        },

                    }
                })
            }).map(command => this.sesClient.send(command))
            .map(output => {
                if (output.$metadata.httpStatusCode == 200) {
                    return undefined;
                }
                console.log("SES returned a code different than 200", output);
                throw new Error(EmailSenderErrors.EMAIL_DELIVERY_FAILED);

            });

    }

    private NotificationEmailAttachmentToSesAttachment(attachment: NotificationEmailAttachment): ResultAsync<Attachment, EmailSenderErrors> {

        return ResultAsync.fromPromise(fetch(attachment.attachmentUrl), error => {
            console.log("Problem while retrieving the attachment with name", attachment.attachmentName, error);
            throw new Error(EmailSenderErrors.ERROR_FETCHING_ATTACHMENT);
        }).map(response => {
            if (!response.ok) {
                console.log("Failed to download attachment", {attachmentName: attachment.attachmentName});
                throw new Error(EmailSenderErrors.ERROR_FETCHING_ATTACHMENT);
            }
            return response.arrayBuffer();
        }).map(arrayBuffer => Buffer.from(arrayBuffer))
            .map(buffer => {
                return {
                    RawContent: buffer,
                    FileName: attachment.attachmentName,
                    ContentType: attachment.attachmentType,
                    ContentDisposition: 'ATTACHMENT',
                    ContentTransferEncoding: "BASE64"
                }
            });
    }
}
