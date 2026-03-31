import IProcessVerificationUseCase from "@Application/UseCases/ProcessVerificationUseCase/IProcessVerificationUseCase";
import IEmailTemplateService from "@Domain/Interfaces/IEmailTemplateService";
import IEmailDeliveryService from "@Domain/Interfaces/IEmailDeliveryService";
import EmailTemplates from "@Domain/ValueObjects/EmailTemplates";
import {CognitoEmailContext} from "@Domain/ValueObjects/CognitoEmailContext";
import {ResultAsync} from "neverthrow";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";

export default class ProcessVerificationUseCaseImpl implements  IProcessVerificationUseCase {

    private readonly EMAIL_SUBJECT = "Verifica tu cuenta"
    
    constructor(private readonly EmailTemplateService: IEmailTemplateService,
                private readonly EmailDeliveryService: IEmailDeliveryService,
                private readonly EmailTemplateNames : EmailTemplates) {}

    public Execute(context: CognitoEmailContext): ResultAsync<void, EmailSenderErrors> {
        return this.EmailTemplateService.RenderTemplate(this.EmailTemplateNames.VerificationTemplate, {
            username: context.username,
            code: context.decryptedCode
        }).andThen(template => this.EmailDeliveryService.Send(context.recipient, this.EMAIL_SUBJECT, template, []))

    }
    
}