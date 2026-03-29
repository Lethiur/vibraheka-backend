import IProcessForgotPasswordUseCase
    from "@Application/UseCases/ProcessForgotPasswordUseCase/IProcessForgotPasswordUseCase";
import {errAsync, ResultAsync} from "neverthrow";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";
import IEmailTemplateService from "@Domain/Interfaces/IEmailTemplateService";
import IEmailDeliveryService from "@Domain/Interfaces/IEmailDeliveryService";
import EmailTemplates from "@Domain/ValueObjects/EmailTemplates";
import {CognitoEmailContext} from "@Domain/ValueObjects/CognitoEmailContext";
import IPasswordResetTokenService from "@Domain/Interfaces/IPasswordResetTokenService";

export default class ProcessForgotPasswordUseCaseImpl implements IProcessForgotPasswordUseCase {

    private readonly EMAIL_SUBJECT = "Tu enlace de recuperacion de contraseña"

    constructor(private readonly EmailTemplateService: IEmailTemplateService,
                private readonly EmailDeliveryService: IEmailDeliveryService,
                private readonly passwordResetTokenService: IPasswordResetTokenService,
                private readonly EmailTemplateNames: EmailTemplates) {
    }

    public Execute(context: CognitoEmailContext): ResultAsync<void, EmailSenderErrors> {

        return this.passwordResetTokenService.BuildPasswordResetToken(
            context.recipient,
            context.decryptedCode
        ).asyncAndThen(resetLink => this.EmailTemplateService.RenderTemplate(this.EmailTemplateNames.PasswordResetTemplate, {
            username: context.username,
            resetLink
        })).andThen(template => this.EmailDeliveryService.Send(context.recipient, this.EMAIL_SUBJECT, template, []))

    }
}