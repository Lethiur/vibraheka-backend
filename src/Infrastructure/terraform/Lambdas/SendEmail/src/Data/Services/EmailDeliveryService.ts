import IEmailDeliveryService from "@Domain/Interfaces/IEmailDeliveryService";
import SESClientWrapper from "@/Clients/SESClient";
import {Result, ResultAsync} from "neverthrow";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";
import {NotificationEmailAttachment} from "@Domain/Entities/NotificationEmailEvent";

/**
 * Service responsible for delivering emails through SES.
 */
export default class EmailDeliveryService implements IEmailDeliveryService {
    constructor(
        private readonly sesClient: SESClientWrapper,
        private readonly fromEmail: string,
        private readonly configurationSetName: string,
        private readonly contactListName: string
    ) {
    }

    /**
     * Sends one HTML email through SES.
     *
     * @param recipient Destination address.
     * @param subject Email subject.
     * @param htmlBody Rendered HTML body.
     * @param attachments An optional list of attachment paths.
     * @returns Async result with success or domain error.
     */
    public Send(recipient: string, subject: string, htmlBody: string, attachments: NotificationEmailAttachment[] = []): ResultAsync<void, EmailSenderErrors> {
        console.log("Sending email through SES", {
            recipient,
            subject,
            attachments
        });
        
        return this.sesClient.sendEmail(
            recipient,
            subject,
            htmlBody,
            this.fromEmail,
            this.configurationSetName,
            attachments
        );
    }

    public AddVerifiedContact(email: string): ResultAsync<void, EmailSenderErrors> {
        console.log("Registering verified contact in SES contact list", {email, contactListName: this.contactListName});
        return this.sesClient.createEmailContact(email, this.contactListName);
    }
}
