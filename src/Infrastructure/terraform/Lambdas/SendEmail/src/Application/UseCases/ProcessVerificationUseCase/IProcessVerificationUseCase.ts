import {CognitoEmailContext} from "@Domain/ValueObjects/CognitoEmailContext";
import {ResultAsync} from "neverthrow";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";

export default interface IProcessVerificationUseCase {

    Execute(context: CognitoEmailContext): ResultAsync<void, EmailSenderErrors>;
    
}