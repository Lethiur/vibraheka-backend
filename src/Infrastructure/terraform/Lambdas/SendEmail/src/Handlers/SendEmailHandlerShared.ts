import {Result} from "neverthrow";
import validateEnvironment from "@/Validators/EnvironmentValidator";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";
import {EnvironmentVariables} from "@/Interfaces/IEnvironmentVariables";
import ResponseEntity from "@Domain/Entities/ResposeEntity";

export function GetEnvironment(): Result<EnvironmentVariables, EmailSenderErrors> {
    return Result.fromThrowable(
        () => validateEnvironment(),
        () => EmailSenderErrors.INVALID_ENVIRONMENT
    )();
}

export function BuildSuccessResponse(triggerSource: string): ResponseEntity {
    return ResponseEntity.Success(triggerSource);
}

export function BuildErrorResponse(error: EmailSenderErrors, triggerSource: string): ResponseEntity {
    console.log("Error processing send-email event", {triggerSource, error});
    return ResponseEntity.Error(error, triggerSource, GetStatusCode(error));
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
