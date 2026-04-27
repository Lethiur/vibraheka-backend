import IProcessVerificationUseCase from "@Application/UseCases/ProcessVerificationUseCase/IProcessVerificationUseCase";
import IEmailTemplateService from "@Domain/Interfaces/IEmailTemplateService";
import IEmailDeliveryService from "@Domain/Interfaces/IEmailDeliveryService";
import EmailTemplates from "@Domain/ValueObjects/EmailTemplates";
import {CognitoEmailContext} from "@Domain/ValueObjects/CognitoEmailContext";
import {ResultAsync} from "neverthrow";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";
import IPasswordResetTokenService from "@Domain/Interfaces/IPasswordResetTokenService";
import SSMClientWrapper from "@/Clients/SSMClient";
import {requireEnv} from "@/Validators/EnvironmentValidator";

export default class ProcessVerificationUseCaseImpl implements  IProcessVerificationUseCase {

    private readonly EMAIL_SUBJECT = "Verifica tu cuenta"
    
    constructor(private readonly EmailTemplateService: IEmailTemplateService,
                private readonly EmailDeliveryService: IEmailDeliveryService,
                private readonly passwordResetTokenService: IPasswordResetTokenService,
                private readonly SSMClient: SSMClientWrapper,
                private readonly EmailTemplateNames: EmailTemplates) {}

    public Execute(context: CognitoEmailContext): ResultAsync<void, EmailSenderErrors> {
        return this.passwordResetTokenService.BuildPasswordResetToken(
            context.recipient,
            context.decryptedCode
        ).asyncAndThen(token => {
            return this.SSMClient.getParameter(requireEnv('SSM_PASSWORD_RESET_FRONTEND_URL'))
                .andThen(url => this.passwordResetTokenService.BuildVerificationLink(token, url))
        })
            .andThen(resetLink => this.EmailTemplateService.RenderTemplate(this.EmailTemplateNames.VerificationTemplate, {
                username: context.username,
                confirmationLink: resetLink
            })).andThen(template => this.EmailDeliveryService.Send(context.recipient, this.EMAIL_SUBJECT, template, []))
            .mapErr(error => {
                console.log("Error while executing the forgot password use case: ", error);
                return EmailSenderErrors.EMAIL_DELIVERY_FAILED;
            })

    }
    
}