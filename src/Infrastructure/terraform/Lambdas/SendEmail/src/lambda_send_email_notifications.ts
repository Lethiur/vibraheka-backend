import {EventBridgeEvent} from "aws-lambda";
import {BuildProcessNotificationEmailUseCase} from "@Domain/Composition/ProcessNotificationEmailComposition";
import {
    BuildErrorResponse,
    BuildSuccessResponse,
    GetEnvironment
} from "@/Handlers/SendEmailHandlerShared";
import NotificationEmailEventDetail from "@Domain/Entities/NotificationEmailEvent";

type NotificationEvent = EventBridgeEvent<"email.notification.requested", NotificationEmailEventDetail>;

export const handler = async (event: NotificationEvent) => {
    const triggerSource = event["detail-type"];

    return GetEnvironment()
        .asyncMap(async environment => {
            console.log("Processing notification email event", {
                detailType: event["detail-type"],
                source: event.source,
                recipient: event.detail?.recipient,
                templateType: event.detail?.templateType
            });

            return BuildProcessNotificationEmailUseCase(environment).Execute(event.detail);
        })
        .match(
            () => BuildSuccessResponse(triggerSource),
            error => BuildErrorResponse(error, triggerSource)
        );
};
