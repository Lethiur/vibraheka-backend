import IProcessRegistrationUseCase from "@Application/UseCases/ProcessRegistrationUseCase/IProcessRegistrationUseCase";
import IEmailTemplateService from "@Domain/Interfaces/IEmailTemplateService";
import IEmailDeliveryService from "@Domain/Interfaces/IEmailDeliveryService";
import EmailTemplates from "@Domain/ValueObjects/EmailTemplates";
import {CognitoEmailContext} from "@Domain/ValueObjects/CognitoEmailContext";
import {ResultAsync} from "neverthrow";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";

export default class ProcessRegistrationUseCaseImpl implements IProcessRegistrationUseCase {

    private readonly EMAIL_SUBJECT = "Bienvenid@ a VibraHeka"
    
    constructor(private readonly EmailTemplateService: IEmailTemplateService,
                private readonly EmailDeliveryService: IEmailDeliveryService,
                private readonly EmailTemplateNames : EmailTemplates) {}

    public Execute(context: CognitoEmailContext): ResultAsync<void, EmailSenderErrors> {
        return this.EmailTemplateService.RenderTemplate(this.EmailTemplateNames.UserWelcomeTemplate, {
            username: context.username,
        }).andThen(template => this.EmailDeliveryService.Send(context.recipient, this.EMAIL_SUBJECT, template, []))
          .andThen(() => this.EmailDeliveryService.AddVerifiedContact(context.recipient))

    }
}