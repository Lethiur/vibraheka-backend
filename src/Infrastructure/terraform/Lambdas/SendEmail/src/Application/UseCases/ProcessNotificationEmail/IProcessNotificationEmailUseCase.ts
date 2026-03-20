import { ResultAsync } from "neverthrow";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";
import { NotificationEmailEventDetail } from "@Domain/Entities/NotificationEmailEvent";

export default interface IProcessNotificationEmailUseCase {
  Execute(event: NotificationEmailEventDetail): ResultAsync<void, EmailSenderErrors>;
}
