import EmailSenderErrors from "@/Domain/Errors/EmailSenderErrors";
import { CognitoEmailContext } from "@/Domain/ValueObjects/CognitoEmailContext";
import IProcessForgotPasswordCompletedUseCase
    from "@Application/UseCases/ProcessForgotPasswordCompletedUseCase/IProcessForgotPasswordCompletedUseCase";
import {ResultAsync} from "neverthrow";
import IEmailTemplateService from "@Domain/Interfaces/IEmailTemplateService";
import IEmailDeliveryService from "@Domain/Interfaces/IEmailDeliveryService";
import EmailTemplates from "@Domain/ValueObjects/EmailTemplates";

export default class ProcessForgotPasswordCompletedUseCaseImpl implements IProcessForgotPasswordCompletedUseCase {
    private readonly EMAIL_SUBJECT = "Tu contraseña ha sido restablecida con exito";

    constructor(private readonly EmailTemplateService: IEmailTemplateService,
                private readonly EmailDeliveryService: IEmailDeliveryService,
                private readonly EmailTemplateNames : EmailTemplates) {}

    public Execute(context : CognitoEmailContext): ResultAsync<void, EmailSenderErrors> {
        return this.EmailTemplateService.RenderTemplate(this.EmailTemplateNames.ForgotPasswordCompletedTemplate, {
            username: context.username
        }).andThen(template => this.EmailDeliveryService.Send(context.recipient, this.EMAIL_SUBJECT, template, []))

    }
    
}