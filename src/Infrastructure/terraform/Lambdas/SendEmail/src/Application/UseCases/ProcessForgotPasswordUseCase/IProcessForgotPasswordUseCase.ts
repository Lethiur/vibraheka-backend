import {ResultAsync} from "neverthrow";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";
import EmailTemplates from "@Domain/ValueObjects/EmailTemplates";
import {CognitoEmailContext} from "@Domain/ValueObjects/CognitoEmailContext";


export default interface IProcessForgotPasswordUseCase {
    
    Execute(context: CognitoEmailContext): ResultAsync<void, EmailSenderErrors>;
}