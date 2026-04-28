import EmailTemplateService from "@Data/Services/EmailTemplateService";
import SSMClientWrapper from "@/Clients/SSMClient";
import S3ClientWrapper from "@/Clients/S3Client";
import {requireEnv} from "@/Validators/EnvironmentValidator";
import EmailDeliveryService from "@Data/Services/EmailDeliveryService";
import SESClientWrapper from "@/Clients/SESClient";
import {EmailTemplatesInstance} from "@Domain/ValueObjects/EmailTemplates";
import IProcessSubscriptionCancelledUseCase
    from "@Application/UseCases/ProcessSubscriptionCancelledUseCase/IProcessSubscriptionCancelledUseCase";
import ProcessSubscriptionCancelledUseCaseImpl
    from "@Application/UseCases/ProcessSubscriptionCancelledUseCase/ProcessSubscriptionCancelledUseCaseImpl";


export const ProcessSubscriptionCancelledUseCase : IProcessSubscriptionCancelledUseCase =
    new ProcessSubscriptionCancelledUseCaseImpl(new EmailTemplateService(
        new SSMClientWrapper(),
        new S3ClientWrapper(),
        requireEnv("TEMPLATE_BUCKET")
    ), new EmailDeliveryService(new SESClientWrapper(), requireEnv("SES_FROM_EMAIL"), requireEnv("SES_CONFIG_SET"), requireEnv("SES_CONTACT_LIST_NAME")), EmailTemplatesInstance);