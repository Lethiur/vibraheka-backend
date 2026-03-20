import {CustomEmailSenderTriggerEvent} from "aws-lambda";
import {BuildProcessCognitoCustomEmailUseCase} from "@Domain/Composition/ProcessCognitoCustomEmailComposition";
import {
    BuildErrorResponse,
    BuildSuccessResponse,
    GetEnvironment
} from "@/Handlers/SendEmailHandlerShared";

export const handler = async (event: CustomEmailSenderTriggerEvent) => {
    const triggerSource = event.triggerSource;

    return GetEnvironment()
        .asyncMap(async environment => {
            console.log("Processing Cognito custom email event", {
                triggerSource: event.triggerSource,
                userName: event.userName
            });

            return BuildProcessCognitoCustomEmailUseCase(environment).Execute(event);
        })
        .match(
            () => BuildSuccessResponse(triggerSource),
            error => BuildErrorResponse(error, triggerSource)
        );
};
