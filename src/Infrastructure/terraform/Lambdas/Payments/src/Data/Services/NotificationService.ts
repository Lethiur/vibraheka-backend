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
        try {
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

            if (result.$metadata.httpStatusCode !== 200) {
                console.error("Problem while sending the email notification event to event bridge, code returned not 200", result);
                return err(EmailNotificationErrors.EmailNotSent);
            }
            console.log("Email notification event sent to event bridge");
            return ok(undefined);    
        } catch (error) {
            console.error("Problem while sending the email notification event to event bridge", error);
            return err(EmailNotificationErrors.EmailNotSent);   
        }
        

    }
}
