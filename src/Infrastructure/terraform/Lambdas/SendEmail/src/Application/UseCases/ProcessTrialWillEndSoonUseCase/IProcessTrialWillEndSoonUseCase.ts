import NotificationEmailEventDetail from "@Domain/Entities/NotificationEmailEvent";
import {ResultAsync} from "neverthrow";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";

export interface IProcessTrialWillEndSoonUseCase {

    Execute(event: NotificationEmailEventDetail): ResultAsync<void, EmailSenderErrors>;
    
}