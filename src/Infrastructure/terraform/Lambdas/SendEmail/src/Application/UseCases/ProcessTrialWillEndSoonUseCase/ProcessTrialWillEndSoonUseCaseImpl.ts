import {
    IProcessTrialWillEndSoonUseCase
} from "@Application/UseCases/ProcessTrialWillEndSoonUseCase/IProcessTrialWillEndSoonUseCase";
import IEmailTemplateService from "@Domain/Interfaces/IEmailTemplateService";
import IEmailDeliveryService from "@Domain/Interfaces/IEmailDeliveryService";
import EmailTemplates from "@Domain/ValueObjects/EmailTemplates";
import NotificationEmailEventDetail from "@Domain/Entities/NotificationEmailEvent";
import {ResultAsync} from "neverthrow";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";

export default class ProcessTrialWillEndSoonUseCaseImpl implements IProcessTrialWillEndSoonUseCase {

    private readonly EMAIL_SUBJECT = "Tu periodo de prueba acabara pronto"

    constructor(private readonly EmailTemplateService: IEmailTemplateService,
                private readonly EmailDeliveryService: IEmailDeliveryService,
                private readonly EmailTemplateNames : EmailTemplates) {}

    public Execute(event: NotificationEmailEventDetail): ResultAsync<void, EmailSenderErrors> {
        return this.EmailTemplateService.RenderTemplate(this.EmailTemplateNames.TrialEndingSoonTemplate, {
            username: event.username
        }).andThen(template => this.EmailDeliveryService.Send(event.recipient, this.EMAIL_SUBJECT, template, event.attachments || []))
    }
}