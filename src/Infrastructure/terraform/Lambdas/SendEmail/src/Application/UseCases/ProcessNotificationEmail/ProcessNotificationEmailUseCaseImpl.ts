import {err, errAsync, ok, Result, ResultAsync} from "neverthrow";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";
import NotificationEmailEventDetail from "@Domain/Entities/NotificationEmailEvent";
import IProcessNotificationEmailUseCase
    from "@Application/UseCases/ProcessNotificationEmail/IProcessNotificationEmailUseCase";
import IEmailDeliveryService from "@Domain/Interfaces/IEmailDeliveryService";
import EmailTemplateService from "@Data/Services/EmailTemplateService";

export default class ProcessNotificationEmailUseCaseImpl implements IProcessNotificationEmailUseCase {
    constructor(
        private readonly templateService: EmailTemplateService,
        private readonly emailDeliveryService: IEmailDeliveryService,
        private readonly templateParameterNames: Record<string, string>
    ) {
    }

    public async Execute(event: NotificationEmailEventDetail): Promise<Result<void, EmailSenderErrors>> {
        const templateParameterName = this.templateParameterNames[event.templateType];
        if (!templateParameterName) {
            return err(EmailSenderErrors.UNSUPPORTED_TRIGGER_SOURCE);
        }

        const resultAsync: Result<string, EmailSenderErrors> = await this.templateService.RenderTemplate(templateParameterName, event.templateData);

        if (resultAsync.isOk()) {
            const htmlBody = resultAsync.value;
            return await this.emailDeliveryService.Send(event.recipient, event.subject, htmlBody, event.attachments || []);
        }
        return resultAsync.map(_ => undefined);
    }
}
