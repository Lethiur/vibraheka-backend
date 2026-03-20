import { errAsync, ResultAsync } from "neverthrow";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";
import { NotificationEmailEventDetail } from "@Domain/Entities/NotificationEmailEvent";
import IProcessNotificationEmailUseCase from "@Application/UseCases/ProcessNotificationEmail/IProcessNotificationEmailUseCase";
import IEmailDeliveryService from "@Domain/Interfaces/IEmailDeliveryService";
import EmailTemplateService from "@Data/Services/EmailTemplateService";

export default class ProcessNotificationEmailUseCaseImpl implements IProcessNotificationEmailUseCase {
  constructor(
    private readonly templateService: EmailTemplateService,
    private readonly emailDeliveryService: IEmailDeliveryService,
    private readonly templateParameterNames: Record<string, string>
  ) {}

  public Execute(event: NotificationEmailEventDetail): ResultAsync<void, EmailSenderErrors> {
    const templateParameterName = this.templateParameterNames[event.templateType];
    if (!templateParameterName) {
      return errAsync(EmailSenderErrors.UNSUPPORTED_TRIGGER_SOURCE);
    }

    return this.templateService
      .RenderTemplate(templateParameterName, event.templateData)
      .andThen((htmlBody) => this.emailDeliveryService.Send(event.recipient, event.subject, htmlBody));
  }
}
