import { EventBridgeClient, PutEventsCommand } from "@aws-sdk/client-eventbridge";

export interface NotificationEmailEventDetail {
  recipient: string;
  subject: string;
  templateType: "subscription_thank_you" | "trial_ending_soon";
  templateData: Record<string, string | number>;
}

export default class NotificationPublisher {
  constructor(
    private readonly eventBridgeClient: EventBridgeClient = new EventBridgeClient(),
    private readonly eventBusName: string = process.env.NOTIFICATION_EVENT_BUS_NAME ?? "default"
  ) {}

  public async publish(detail: NotificationEmailEventDetail): Promise<void> {
    await this.eventBridgeClient.send(
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
  }
}
