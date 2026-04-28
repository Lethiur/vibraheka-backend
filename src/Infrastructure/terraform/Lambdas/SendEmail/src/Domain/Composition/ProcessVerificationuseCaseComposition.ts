import {EmailTemplatesInstance} from "@Domain/ValueObjects/EmailTemplates";
import IProcessVerificationUseCase from "@Application/UseCases/ProcessVerificationUseCase/IProcessVerificationUseCase";
import ProcessVerificationUseCaseImpl
    from "@Application/UseCases/ProcessVerificationUseCase/ProcessVerificationUseCaseImpl";
import EmailTemplateService from "@Data/Services/EmailTemplateService";
import EmailDeliveryService from "@Data/Services/EmailDeliveryService";
import SSMClientWrapper from "@/Clients/SSMClient";
import S3ClientWrapper from "@/Clients/S3Client";
import {requireEnv} from "@/Validators/EnvironmentValidator";
import SESClientWrapper from "@/Clients/SESClient";
import PasswordResetTokenService from "@Data/Services/PasswordResetTokenService";

export const ProcessVerificationUseCase : IProcessVerificationUseCase = new ProcessVerificationUseCaseImpl(
    new EmailTemplateService(new SSMClientWrapper(),new S3ClientWrapper(),requireEnv("TEMPLATE_BUCKET")),
    new EmailDeliveryService(new SESClientWrapper(), requireEnv("SES_FROM_EMAIL"), requireEnv("SES_CONFIG_SET")),
    new PasswordResetTokenService(requireEnv('PASSWORD_RESET_TOKEN_SECRET'), parseInt(requireEnv('PASSWORD_RESET_TOKEN_TTL_MINUTES'))),
    new SSMClientWrapper(),
    EmailTemplatesInstance);