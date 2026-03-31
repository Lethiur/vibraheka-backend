import {EventBridgeEvent} from "aws-lambda";
import NotificationEmailEventDetail from "@Domain/Entities/NotificationEmailEvent";
import {ProcessSubscriptionThankYouUseCase} from "@Domain/Composition/ProcessSubscriptionThankYouUseCaseComposition";
import {err, errAsync, ResultAsync} from "neverthrow";
import EmailSenderErrors from "./Domain/Errors/EmailSenderErrors";
import {
    processSubscriptionReactivatedUseCase
} from "@Domain/Composition/ProcessSubscriptionReactivatedUseCaseComposition";
import {ProcessSubscriptionCancelledUseCase} from "@Domain/Composition/ProcessSubscriptionCancelledUseCaseComposition";
import {ProcessTrialWillEndSoonUseCase} from "@Domain/Composition/ProcessTrialWillEndSoonUseCaseComposition";
import {BuildErrorResponse, BuildSuccessResponse} from "@/Handlers/SendEmailHandlerShared";

type NotificationEvent = EventBridgeEvent<"email.notification.requested", NotificationEmailEventDetail>;

export const handler = async (event: NotificationEvent) => {
    const triggerSource = event["detail-type"];
    const eventDetail = event["detail"];
    
    let result: ResultAsync<void, EmailSenderErrors>;

    switch (eventDetail.templateType) {
        case "subscription_thank_you":
            result = ProcessSubscriptionThankYouUseCase.Execute(eventDetail);
            break;
        case "subscription_reactivated":
            result = processSubscriptionReactivatedUseCase.Execute(eventDetail);
            break;
        case "subscription_cancelled":
            result = ProcessSubscriptionCancelledUseCase.Execute(eventDetail);
            break;
        case "trial_ending_soon":
            result = ProcessTrialWillEndSoonUseCase.Execute(eventDetail);
            break;
        default:
            result = errAsync(EmailSenderErrors.INVALID_EVENT);
            break;
    }
    
    return await result.match(_ => BuildSuccessResponse(eventDetail.templateType), 
            err => BuildErrorResponse(err, eventDetail.templateType));
};
