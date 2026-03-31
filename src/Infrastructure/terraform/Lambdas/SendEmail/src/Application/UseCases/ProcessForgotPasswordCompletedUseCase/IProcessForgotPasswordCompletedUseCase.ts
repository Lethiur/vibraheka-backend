import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";
import {ResultAsync} from "neverthrow"
import {CognitoEmailContext} from "@Domain/ValueObjects/CognitoEmailContext"; 

export default interface IProcessForgotPasswordCompletedUseCase {
    Execute(context: CognitoEmailContext): ResultAsync<void, EmailSenderErrors>;
}