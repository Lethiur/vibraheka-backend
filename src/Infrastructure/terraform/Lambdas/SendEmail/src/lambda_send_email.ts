import {CustomEmailSenderTriggerEvent, CustomEmailSenderTriggerHandler} from "aws-lambda";
import {Result} from "neverthrow";
import validateEnvironment from "@/Validators/EnvironmentValidator";
import {BuildProcessCognitoCustomEmailUseCase} from "@Domain/Composition/ProcessCognitoCustomEmailComposition";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";
import {EnvironmentVariables} from "@/Interfaces/IEnvironmentVariables";

/**
 * Lambda entrypoint for Cognito CustomEmailSender events.
 * It validates configuration, executes the use case and maps Result to HTTP-like response.
 *
 * @param event Cognito trigger payload.
 * @returns HTTP-like response expected by lambda runtime.
 */
export const handler: CustomEmailSenderTriggerHandler = async (
    event: CustomEmailSenderTriggerEvent
) => {
    const environmentResult = GetEnvironment();
    if (environmentResult.isErr()) {
        console.error("Environment validation failed for send-email lambda", {
            error: environmentResult.error,
            triggerSource: event.triggerSource
        });
        return {
            statusCode: GetStatusCode(environmentResult.error),
            body: JSON.stringify({
                success: false,
                error: environmentResult.error,
                triggerSource: event.triggerSource
            })
        };
    }

    const useCase = BuildProcessCognitoCustomEmailUseCase(environmentResult.value);
    console.log("Processing Cognito custom email event", {
        triggerSource: event.triggerSource,
        userName: event.userName
    });
    const executionResult = await useCase.Execute(event);

    if (executionResult.isErr()) {
        const statusCode = GetStatusCode(executionResult.error);
        console.error("Error processing Cognito custom email event", {
            error: executionResult.error,
            statusCode,
            triggerSource: event.triggerSource
        });

        return {
            statusCode,
            body: JSON.stringify({
                success: false,
                error: executionResult.error,
                triggerSource: event.triggerSource
            })
        };
    }

    console.log("Cognito custom email event processed successfully", {
        triggerSource: event.triggerSource,
        userName: event.userName
    });
    return {
        statusCode: 200,
        body: JSON.stringify({
            success: true,
            message: "Email processed successfully",
            triggerSource: event.triggerSource
        })
    };
};

/**
 * Loads and validates lambda environment values.
 *
 * @returns Result with valid environment configuration or error.
 */
function GetEnvironment(): Result<EnvironmentVariables, EmailSenderErrors> {
    return Result.fromThrowable(
        () => validateEnvironment(),
        () => EmailSenderErrors.INVALID_ENVIRONMENT
    )();
}

/**
 * Maps domain errors to response status codes.
 *
 * @param error Domain error returned by the use case.
 * @returns Numeric status code used in lambda response.
 */
function GetStatusCode(error: EmailSenderErrors): number {
    switch (error) {
        case EmailSenderErrors.INVALID_EVENT:
        case EmailSenderErrors.INVALID_ENVIRONMENT:
        case EmailSenderErrors.MISSING_RECIPIENT:
        case EmailSenderErrors.MISSING_CODE:
        case EmailSenderErrors.UNSUPPORTED_TRIGGER_SOURCE:
            return 400;
        default:
            return 500;
    }
}
