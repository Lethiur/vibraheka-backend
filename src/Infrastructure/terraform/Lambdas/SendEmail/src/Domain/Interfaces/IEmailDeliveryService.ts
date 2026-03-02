import {ResultAsync} from "neverthrow";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";

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
     * @returns Async result containing success or a domain error.
     */
    Send(recipient: string, subject: string, htmlBody: string): ResultAsync<void, EmailSenderErrors>;
}
