import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";
import ResponseEntity from "@Domain/Entities/ResposeEntity";
import {CognitoEmailContext} from "@Domain/ValueObjects/CognitoEmailContext";
import {errAsync, ResultAsync} from "neverthrow";
import {CustomEmailSenderTriggerEvent} from "aws-lambda";
import ICognitoCodeCipherService from "@Domain/Interfaces/ICognitoCodeCipherService";

export function BuildSuccessResponse(triggerSource: string): ResponseEntity {
    return ResponseEntity.Success(triggerSource);
}

export function BuildErrorResponse(error: EmailSenderErrors, triggerSource: string): ResponseEntity {
    console.log("Error processing send-email event", {triggerSource, error});
    return ResponseEntity.Error(error, triggerSource, GetStatusCode(error));
}

/**
 * Extracts, validates and decrypts event fields into a unified email context.
 *
 * @param event Cognito event payload.
 * @param cipherService Service for decrypting Cognito encrypted codes.
 * @returns Async result with normalized context or domain error.
 */
export function BuildContext(event: CustomEmailSenderTriggerEvent, cipherService: ICognitoCodeCipherService): ResultAsync<CognitoEmailContext, EmailSenderErrors> {
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
    return cipherService.DecryptCode(encryptedCode)
        .map(decryptedCode => ({
            recipient,
            username,
            decryptedCode,
            triggerSource: event.triggerSource
        }));
}

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
