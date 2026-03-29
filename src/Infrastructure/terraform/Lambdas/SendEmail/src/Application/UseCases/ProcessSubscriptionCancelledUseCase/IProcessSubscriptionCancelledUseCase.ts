import {CognitoEmailContext} from "@Domain/ValueObjects/CognitoEmailContext";
import {ResultAsync} from "neverthrow";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";
import NotificationEmailEventDetail from "@Domain/Entities/NotificationEmailEvent";

export default interface IProcessSubscriptionCancelledUseCase {

    Execute(event: NotificationEmailEventDetail): ResultAsync<void, EmailSenderErrors>;
    
}