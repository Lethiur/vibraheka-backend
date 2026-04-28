import {CustomEmailSenderTriggerEvent} from "aws-lambda";
import {
    BuildContext,
} from "@/Handlers/SendEmailHandlerShared";
import {CognitoEmailContext} from "@Domain/ValueObjects/CognitoEmailContext";
import CognitoCodeCipherService from "@Data/Services/CognitoCodeCipherService";
import {requireEnv} from "./Validators/EnvironmentValidator";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";
import {ResultAsync} from "neverthrow";
import {ProcessVerificationUseCase} from "@Domain/Composition/ProcessVerificationuseCaseComposition";
import {ProcessForgotPasswordUseCase} from "@Domain/Composition/ProcessForgotPasswordUseCaseComposition";
import {ProcessRegistrationUseCase} from "@Domain/Composition/ProcessUserWelcomeUseCaseComposition";
import {
    ProcessForgotPasswordCompletedUseCase
} from "@Domain/Composition/ProcessForgotPasswordCompletedUseCaseComposition";

export const handler = async (event: CustomEmailSenderTriggerEvent) => {
    const triggerSource = event.triggerSource;

    const context: ResultAsync<CognitoEmailContext, EmailSenderErrors> = BuildContext(event, new CognitoCodeCipherService(
        requireEnv('KEY_ALIAS'), requireEnv('KEY_ARN')
    ));

    return await context.andThen(context => {
        switch (context.triggerSource) {
            case "CustomEmailSender_SignUp":
            case "CustomEmailSender_ResendCode":
                console.log("Routing to verification email flow", {
                    recipient: context.recipient,
                    triggerSource: context.triggerSource
                });
                return ProcessVerificationUseCase.Execute(context);
            case "CustomEmailSender_ForgotPassword":
                console.log("Routing to password-reset email flow", {
                    recipient: context.recipient,
                    triggerSource: context.triggerSource
                });
                return ProcessForgotPasswordUseCase.Execute(context);
            case "PostConfirmation_ConfirmSignUp":
                console.log("Routing to post-confirmation email flow", {
                    recipient: context.recipient,
                    triggerSource: context.triggerSource
                });
                return ProcessRegistrationUseCase.Execute(context);
            case "PostConfirmation_ConfirmForgotPassword":
                console.log("Routing to forgot-password email flow", {
                    recipient: context.recipient,
                    triggerSource: context.triggerSource
                });
                return ProcessForgotPasswordCompletedUseCase.Execute(context);
            default:
                console.error("Unsupported Cognito trigger source", {
                    triggerSource: context.triggerSource
                });
                throw new Error("Unsupported Cognito trigger source");
        }
    }).match(
        _ => {
            // Cognito triggers expect the event back (not an API Gateway-style response).
            console.log("Cognito trigger handled successfully", {triggerSource});
            return event;
        },
        error => {
            // If you prefer to fail the Cognito flow, change this to: `throw new Error(String(error))`.
            console.error("Cognito trigger failed; returning event anyway", {triggerSource, error});
            return event;
        }
    );

};
