import {ResultAsync} from "neverthrow";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";

/**
 * 
 */
export default interface IProcessForgotPasswordUseCase {
    
    Execute(username: string, email : string): ResultAsync<void, EmailSenderErrors>;
}