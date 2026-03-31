import EmailTemplateService from "@Data/Services/EmailTemplateService";
import SSMClientWrapper from "@/Clients/SSMClient";
import S3ClientWrapper from "@/Clients/S3Client";
import {requireEnv} from "@/Validators/EnvironmentValidator";
import EmailDeliveryService from "@Data/Services/EmailDeliveryService";
import SESClientWrapper from "@/Clients/SESClient";
import {EmailTemplatesInstance} from "@Domain/ValueObjects/EmailTemplates";
import IProcessSubscriptionReactivatedUseCase
    from "@Application/UseCases/ProcessSubscriptionReactivatedUseCase/IProcessSubscriptionReactivatedUseCase";
import ProcessSubscriptionReactivatedUseCaseImpl
    from "@Application/UseCases/ProcessSubscriptionReactivatedUseCase/ProcessSubscriptionReactivatedUseCaseImpl";


export const processSubscriptionReactivatedUseCase : IProcessSubscriptionReactivatedUseCase =
    new ProcessSubscriptionReactivatedUseCaseImpl(new EmailTemplateService(
        new SSMClientWrapper(),
        new S3ClientWrapper(),
        requireEnv("TEMPLATE_BUCKET")
    ), new EmailDeliveryService(new SESClientWrapper(), requireEnv("SES_FROM_EMAIL"), requireEnv("SES_CONFIG_SET")), EmailTemplatesInstance);