import IProcessForgotPasswordUseCase
    from "@Application/UseCases/ProcessForgotPasswordUseCase/IProcessForgotPasswordUseCase";
import {errAsync, ResultAsync} from "neverthrow";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";
import IEmailTemplateService from "@Domain/Interfaces/IEmailTemplateService";
import IEmailDeliveryService from "@Domain/Interfaces/IEmailDeliveryService";
import EmailTemplates from "@Domain/ValueObjects/EmailTemplates";
import {CognitoEmailContext} from "@Domain/ValueObjects/CognitoEmailContext";
import IPasswordResetTokenService from "@Domain/Interfaces/IPasswordResetTokenService";
import SSMClientWrapper from "@/Clients/SSMClient";
import {requireEnv} from "@/Validators/EnvironmentValidator";

export default class ProcessForgotPasswordUseCaseImpl implements IProcessForgotPasswordUseCase {

    private readonly EMAIL_SUBJECT = "Tu enlace de recuperacion de contraseña"

    constructor(private readonly EmailTemplateService: IEmailTemplateService,
                private readonly EmailDeliveryService: IEmailDeliveryService,
                private readonly passwordResetTokenService: IPasswordResetTokenService,
                private readonly SSMClient: SSMClientWrapper,
                private readonly EmailTemplateNames: EmailTemplates) {
    }

    public Execute(context: CognitoEmailContext): ResultAsync<void, EmailSenderErrors> {

        return this.passwordResetTokenService.BuildPasswordResetToken(
            context.recipient,
            context.decryptedCode
        ).asyncAndThen(token => {
            return this.SSMClient.getParameter(requireEnv('SSM_PASSWORD_RESET_FRONTEND_URL'))
                .andThen(url => this.passwordResetTokenService.BuildPasswordResetLink(token, url))
        })
            .andThen(resetLink => this.EmailTemplateService.RenderTemplate(this.EmailTemplateNames.PasswordResetTemplate, {
            username: context.username,
            resetLink
        })).andThen(template => this.EmailDeliveryService.Send(context.recipient, this.EMAIL_SUBJECT, template, []))
            .mapErr(error => {
                console.log("Error while executing the forgot password use case: ", error);
                return EmailSenderErrors.EMAIL_DELIVERY_FAILED;
            })

    }
}