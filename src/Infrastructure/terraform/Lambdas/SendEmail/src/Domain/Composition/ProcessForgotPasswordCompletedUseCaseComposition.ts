import {EmailTemplatesInstance} from "@Domain/ValueObjects/EmailTemplates";
import EmailTemplateService from "@Data/Services/EmailTemplateService";
import EmailDeliveryService from "@Data/Services/EmailDeliveryService";
import SSMClientWrapper from "@/Clients/SSMClient";
import S3ClientWrapper from "@/Clients/S3Client";
import {requireEnv} from "@/Validators/EnvironmentValidator";
import SESClientWrapper from "@/Clients/SESClient";
import ProcessForgotPasswordCompletedUseCaseImpl
    from "../../Application/UseCases/ProcessForgotPasswordCompletedUseCase/ProcessForgotPasswordCompletedUseCaseImpl";
import IProcessForgotPasswordCompletedUseCase
    from "../../Application/UseCases/ProcessForgotPasswordCompletedUseCase/IProcessForgotPasswordCompletedUseCase";

export const ProcessForgotPasswordCompletedUseCase : IProcessForgotPasswordCompletedUseCase = new ProcessForgotPasswordCompletedUseCaseImpl(
    new EmailTemplateService(new SSMClientWrapper(),new S3ClientWrapper(),requireEnv("TEMPLATE_BUCKET")),
    new EmailDeliveryService(new SESClientWrapper(), requireEnv("SES_FROM_EMAIL"), requireEnv("SES_CONFIG_SET")),
    EmailTemplatesInstance);