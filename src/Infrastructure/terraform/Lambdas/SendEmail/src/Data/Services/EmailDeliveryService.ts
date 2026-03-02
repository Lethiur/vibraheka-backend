import IEmailDeliveryService from "@Domain/Interfaces/IEmailDeliveryService";
import SESClientWrapper from "@/Clients/SESClient";
import {ResultAsync} from "neverthrow";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";

/**
 * Service responsible for delivering emails through SES.
 */
export default class EmailDeliveryService implements IEmailDeliveryService {
    constructor(
        private readonly sesClient: SESClientWrapper,
        private readonly fromEmail: string,
        private readonly configurationSetName: string
    ) {
    }

    /**
     * Sends one HTML email through SES.
     *
     * @param recipient Destination address.
     * @param subject Email subject.
     * @param htmlBody Rendered HTML body.
     * @returns Async result with success or domain error.
     */
    public Send(recipient: string, subject: string, htmlBody: string): ResultAsync<void, EmailSenderErrors> {
        console.log("Sending email through SES", {
            recipient,
            subject
        });
        return ResultAsync.fromPromise(this.sesClient.sendEmail(
            recipient,
            subject,
            htmlBody,
            this.fromEmail,
            this.configurationSetName
        ), (error) => {
            console.error("Failed sending email through SES", {
                recipient,
                subject,
                error
            });
            return EmailSenderErrors.EMAIL_DELIVERY_FAILED;
        });
    }
}
