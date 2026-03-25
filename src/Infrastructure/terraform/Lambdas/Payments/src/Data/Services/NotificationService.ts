import {EventBridgeClient, PutEventsCommand, PutEventsCommandOutput} from "@aws-sdk/client-eventbridge";
import INotificationService from "@Domain/Interfaces/INotificationService";
import NotificationEmailEventDetail from "@Domain/Events/NotificationEmailEvent";
import {err, ok, Result} from "neverthrow";
import {EmailNotificationErrors} from "@Domain/Errors/EmailNotificationErrors";


export default class NotificationService implements INotificationService {
    constructor(
        private readonly eventBridgeClient: EventBridgeClient = new EventBridgeClient(),
        private readonly eventBusName: string = process.env.NOTIFICATION_EVENT_BUS_NAME ?? "default"
    ) {
    }

    public async Publish(detail: NotificationEmailEventDetail): Promise<Result<void, EmailNotificationErrors>> {
        const result: PutEventsCommandOutput = await this.eventBridgeClient.send(
            new PutEventsCommand({
                Entries: [
                    {
                        EventBusName: this.eventBusName,
                        Source: "vibraheka.payments",
                        DetailType: "email.notification.requested",
                        Detail: JSON.stringify(detail),
                    },
                ],
            })
        );

        if (result.$metadata.httpStatusCode === 200) {
            return ok(undefined);
        }

        return err(EmailNotificationErrors.EmailNotSent);

    }
}
