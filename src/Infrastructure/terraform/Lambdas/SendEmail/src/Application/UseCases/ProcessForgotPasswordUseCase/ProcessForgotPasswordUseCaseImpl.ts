import IProcessForgotPasswordUseCase
    from "@Application/UseCases/ProcessForgotPasswordUseCase/IProcessForgotPasswordUseCase";
import {ResultAsync} from "neverthrow";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";

export default class ProcessForgotPasswordUseCaseImpl implements IProcessForgotPasswordUseCase {
    
    constructor() {
    }
    
    public Execute(username: string, email : string): ResultAsync<void, EmailSenderErrors> {
        
    }
}