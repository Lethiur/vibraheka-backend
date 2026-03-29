import IProcessSubscriptionReactivatedUseCase
    from "@Application/UseCases/ProcessSubscriptionReactivatedUseCase/IProcessSubscriptionReactivatedUseCase";
import IEmailTemplateService from "@Domain/Interfaces/IEmailTemplateService";
import IEmailDeliveryService from "@Domain/Interfaces/IEmailDeliveryService";
import EmailTemplates from "@Domain/ValueObjects/EmailTemplates";
import {CognitoEmailContext} from "@Domain/ValueObjects/CognitoEmailContext";
import {ResultAsync} from "neverthrow";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";
import NotificationEmailEventDetail from "@Domain/Entities/NotificationEmailEvent";

export default class ProcessSubscriptionReactivatedUseCaseImpl implements IProcessSubscriptionReactivatedUseCase {

    private readonly EMAIL_SUBJECT = "Tu subscripcion ha sido reactivada"
    
    constructor(private readonly EmailTemplateService: IEmailTemplateService,
                private readonly EmailDeliveryService: IEmailDeliveryService,
                private readonly EmailTemplateNames : EmailTemplates) {}

    public Execute(event: NotificationEmailEventDetail): ResultAsync<void, EmailSenderErrors> {
        return this.EmailTemplateService.RenderTemplate(this.EmailTemplateNames.SubscriptionReactivatedTemplate, {
            username: context.username
        }).andThen(template => this.EmailDeliveryService.Send(context.recipient, this.EMAIL_SUBJECT, template, []))
    }
}