import NotificationEmailEventDetail from "@Domain/Events/NotificationEmailEvent";
import {Result} from "neverthrow";
import {EmailNotificationErrors} from "@Domain/Errors/EmailNotificationErrors";

export default interface INotificationService {
    Publish(detail: NotificationEmailEventDetail): Promise<Result<void, EmailNotificationErrors>>
}