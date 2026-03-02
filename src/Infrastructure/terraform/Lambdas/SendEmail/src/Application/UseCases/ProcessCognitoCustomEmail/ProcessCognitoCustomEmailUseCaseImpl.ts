import {CustomEmailSenderTriggerEvent} from "aws-lambda";
import {errAsync, ResultAsync} from "neverthrow";
import ICognitoCodeCipherService from "@Domain/Interfaces/ICognitoCodeCipherService";
import IEmailDeliveryService from "@Domain/Interfaces/IEmailDeliveryService";
import IEmailTemplateService from "@Domain/Interfaces/IEmailTemplateService";
import IPasswordResetTokenService from "@Domain/Interfaces/IPasswordResetTokenService";
import IProcessCognitoCustomEmailUseCase from "@Application/UseCases/ProcessCognitoCustomEmail/IProcessCognitoCustomEmailUseCase";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";
import {CognitoEmailContext} from "@Domain/Entities/CognitoEmailContext";

/**
 * Use case that handles Cognito CustomEmailSender events for signup verification and forgot-password.
 */
export default class ProcessCognitoCustomEmailUseCaseImpl implements IProcessCognitoCustomEmailUseCase {
    constructor(
        private readonly codeCipherService: ICognitoCodeCipherService,
        private readonly templateService: IEmailTemplateService,
        private readonly emailDeliveryService: IEmailDeliveryService,
        private readonly passwordResetTokenService: IPasswordResetTokenService
    ) {
    }

    /**
     * Routes the event to the corresponding email flow and executes it.
     *
     * @param event Cognito CustomEmailSender event.
     * @returns Async result with success or domain error.
     */
    public Execute(event: CustomEmailSenderTriggerEvent): ResultAsync<void, EmailSenderErrors> {
        console.log("Executing Cognito custom email use case", {
            triggerSource: event.triggerSource,
            userName: event.userName
        });
        return this.BuildContext(event)
            .andThen(context => {
                switch (context.triggerSource) {
                    case "CustomEmailSender_SignUp":
                    case "CustomEmailSender_ResendCode":
                    case "CustomEmailSender_VerifyUserAttribute":
                        console.log("Routing to verification email flow", {
                            recipient: context.recipient,
                            triggerSource: context.triggerSource
                        });
                        return this.SendVerificationEmail(context);
                    case "CustomEmailSender_ForgotPassword":
                        console.log("Routing to password-reset email flow", {
                            recipient: context.recipient,
                            triggerSource: context.triggerSource
                        });
                        return this.SendPasswordResetEmail(context);
                    default:
                        console.error("Unsupported Cognito trigger source", {
                            triggerSource: context.triggerSource
                        });
                        return errAsync(EmailSenderErrors.UNSUPPORTED_TRIGGER_SOURCE);
                }
            });
    }

    /**
     * Extracts, validates and decrypts event fields into a unified email context.
     *
     * @param event Cognito event payload.
     * @returns Async result with normalized context or domain error.
     */
    private BuildContext(event: CustomEmailSenderTriggerEvent): ResultAsync<CognitoEmailContext, EmailSenderErrors> {
        if (!event?.request) {
            console.error("Invalid Cognito event: request is missing");
            return errAsync(EmailSenderErrors.INVALID_EVENT);
        }

        const userAttributes = event.request.userAttributes as Record<string, string> | undefined;
        const recipient = userAttributes?.["email"] ?? event.userName;

        if (!recipient || recipient.trim().length === 0) {
            console.error("Missing recipient in Cognito event", {userName: event.userName});
            return errAsync(EmailSenderErrors.MISSING_RECIPIENT);
        }

        const encryptedCode = event.request.code;
        if (!encryptedCode || encryptedCode.trim().length === 0) {
            console.error("Missing encrypted code in Cognito event", {recipient});
            return errAsync(EmailSenderErrors.MISSING_CODE);
        }

        const username = userAttributes?.["name"] ?? recipient;
        return this.codeCipherService.DecryptCode(encryptedCode)
            .map(decryptedCode => ({
                recipient,
                username,
                decryptedCode,
                triggerSource: event.triggerSource
            }));
    }

    /**
     * Sends verification code email for signup/resend/verify attribute triggers.
     *
     * @param context Normalized event context.
     * @returns Async result with success or domain error.
     */
    private SendVerificationEmail(context: CognitoEmailContext): ResultAsync<void, EmailSenderErrors> {
        return this.templateService.RenderVerificationTemplate(
            context.username,
            context.decryptedCode
        ).andThen(htmlBody => {
            console.log("Verification template rendered", {recipient: context.recipient});
            return this.emailDeliveryService.Send(
                context.recipient,
                "Tu codigo de verificacion",
                htmlBody
            );
        });
    }

    /**
     * Sends password-reset email with proprietary token and reset link.
     *
     * @param context Normalized event context.
     * @returns Async result with success or domain error.
     */
    private SendPasswordResetEmail(context: CognitoEmailContext): ResultAsync<void, EmailSenderErrors> {
        const tokenResult = this.passwordResetTokenService.BuildPasswordResetToken(
            context.recipient,
            context.decryptedCode
        );

        if (tokenResult.isErr()) {
            console.error("Failed to build password reset token", {recipient: context.recipient, error: tokenResult.error});
            return errAsync(tokenResult.error);
        }

        const resetLinkResult = this.passwordResetTokenService.BuildPasswordResetLink(tokenResult.value);
        if (resetLinkResult.isErr()) {
            console.error("Failed to build password reset link", {recipient: context.recipient, error: resetLinkResult.error});
            return errAsync(resetLinkResult.error);
        }

        return this.templateService.RenderPasswordResetTemplate(
            context.username,
            tokenResult.value,
            resetLinkResult.value
        ).andThen(htmlBody => {
            console.log("Password reset template rendered", {recipient: context.recipient});
            return this.emailDeliveryService.Send(
                context.recipient,
                "Recupera tu contrasena",
                htmlBody
            );
        });
    }
}
