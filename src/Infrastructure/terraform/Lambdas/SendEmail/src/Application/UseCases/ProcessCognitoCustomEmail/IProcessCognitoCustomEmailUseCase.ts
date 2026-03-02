import {CustomEmailSenderTriggerEvent} from "aws-lambda";
import {ResultAsync} from "neverthrow";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";

/**
 * Contract for processing Cognito CustomEmailSender events.
 */
export default interface IProcessCognitoCustomEmailUseCase {
    /**
     * Processes one Cognito event and sends the corresponding email.
     *
     * @param event Cognito CustomEmailSender event payload.
     * @returns Async result containing success or a domain error.
     */
    Execute(event: CustomEmailSenderTriggerEvent): ResultAsync<void, EmailSenderErrors>;
}
