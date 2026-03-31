import IProcessSubscriptionThankYouUseCase
    from "@Application/UseCases/ProcessSubscriptionThankYouUseCase/IProcessSubscriptionThankYouUseCase";
import IEmailTemplateService from "@Domain/Interfaces/IEmailTemplateService";
import IEmailDeliveryService from "@Domain/Interfaces/IEmailDeliveryService";
import EmailTemplates from "@Domain/ValueObjects/EmailTemplates";
import {CognitoEmailContext} from "@Domain/ValueObjects/CognitoEmailContext";
import {ResultAsync} from "neverthrow";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";
import NotificationEmailEventDetail from "@Domain/Entities/NotificationEmailEvent";

export default class ProcessSubscriptionThankYouUseCaseImpl implements IProcessSubscriptionThankYouUseCase {

    private readonly EMAIL_SUBJECT = "Muchas gracias por tu subscripcion"
    
    constructor(private readonly EmailTemplateService: IEmailTemplateService,
                private readonly EmailDeliveryService: IEmailDeliveryService,
                private readonly EmailTemplateNames : EmailTemplates) {}

    public Execute(event: NotificationEmailEventDetail): ResultAsync<void, EmailSenderErrors> {
        return this.EmailTemplateService.RenderTemplate(this.EmailTemplateNames.SubscriptionThankYouTemplate, {
            username: event.username
        }).andThen(template => this.EmailDeliveryService.Send(event.recipient, this.EMAIL_SUBJECT, template, event.attachments || []))
    }
}