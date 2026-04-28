import EmailTemplateService from "@Data/Services/EmailTemplateService";
import SSMClientWrapper from "@/Clients/SSMClient";
import S3ClientWrapper from "@/Clients/S3Client";
import {requireEnv} from "@/Validators/EnvironmentValidator";
import EmailDeliveryService from "@Data/Services/EmailDeliveryService";
import SESClientWrapper from "@/Clients/SESClient";
import {EmailTemplatesInstance} from "@Domain/ValueObjects/EmailTemplates";
import ProcessTrialWillEndSoonUseCaseImpl
    from "@Application/UseCases/ProcessTrialWillEndSoonUseCase/ProcessTrialWillEndSoonUseCaseImpl";
import {
    IProcessTrialWillEndSoonUseCase
} from "@Application/UseCases/ProcessTrialWillEndSoonUseCase/IProcessTrialWillEndSoonUseCase";


export const ProcessTrialWillEndSoonUseCase : IProcessTrialWillEndSoonUseCase =
    new ProcessTrialWillEndSoonUseCaseImpl(new EmailTemplateService(
        new SSMClientWrapper(),
        new S3ClientWrapper(),
        requireEnv("TEMPLATE_BUCKET")
    ), new EmailDeliveryService(new SESClientWrapper(), requireEnv("SES_FROM_EMAIL"), requireEnv("SES_CONFIG_SET"), requireEnv("SES_CONTACT_LIST_NAME")), EmailTemplatesInstance);