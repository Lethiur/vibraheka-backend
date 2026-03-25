import {Result, ResultAsync} from "neverthrow";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";
import {NotificationEmailAttachment} from "@Domain/Entities/NotificationEmailEvent";

/**
 * Contract for sending emails through an external provider.
 */
export default interface IEmailDeliveryService {
    /**
     * Sends a rendered HTML email.
     *
     * @param recipient Destination email address.
     * @param subject Email subject.
     * @param htmlBody Rendered HTML body.
     * @param attachments The list of attachment to send with the email
     * @returns Async result containing success or a domain error.
     */
    Send(recipient: string, subject: string, htmlBody: string, attachments: NotificationEmailAttachment[]): Promise<Result<void, EmailSenderErrors>>
}
