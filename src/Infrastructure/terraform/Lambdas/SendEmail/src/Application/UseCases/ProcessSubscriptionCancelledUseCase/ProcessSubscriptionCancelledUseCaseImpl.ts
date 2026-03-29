import IProcessSubscriptionCancelledUseCase
    from "@Application/UseCases/ProcessSubscriptionCancelledUseCase/IProcessSubscriptionCancelledUseCase";
import IEmailTemplateService from "@Domain/Interfaces/IEmailTemplateService";
import IEmailDeliveryService from "@Domain/Interfaces/IEmailDeliveryService";
import EmailTemplates from "@Domain/ValueObjects/EmailTemplates";
import {CognitoEmailContext} from "@Domain/ValueObjects/CognitoEmailContext";
import {ResultAsync} from "neverthrow";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";
import NotificationEmailEventDetail from "@Domain/Entities/NotificationEmailEvent";

export default class ProcessSubscriptionCancelledUseCaseImpl implements IProcessSubscriptionCancelledUseCase {

    private readonly EMAIL_SUBJECT = "Tu subscripcion ha sido cancelada";

    constructor(private readonly EmailTemplateService: IEmailTemplateService,
                private readonly EmailDeliveryService: IEmailDeliveryService,
                private readonly EmailTemplateNames : EmailTemplates) {}

    public Execute(event: NotificationEmailEventDetail): ResultAsync<void, EmailSenderErrors> {
        return this.EmailTemplateService.RenderTemplate(this.EmailTemplateNames.SubscriptionCancelledTemplate, {
            username: context.username
        }).andThen(template => this.EmailDeliveryService.Send(context.recipient, this.EMAIL_SUBJECT, template, []))

    }
}