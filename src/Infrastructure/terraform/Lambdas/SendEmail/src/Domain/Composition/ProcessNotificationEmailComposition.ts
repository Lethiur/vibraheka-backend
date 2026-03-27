import SESClientWrapper from "@/Clients/SESClient";
import S3ClientWrapper from "@/Clients/S3Client";
import SSMClientWrapper from "@/Clients/SSMClient";
import { EnvironmentVariables } from "@/Interfaces/IEnvironmentVariables";
import ProcessNotificationEmailUseCaseImpl from "@Application/UseCases/ProcessNotificationEmail/ProcessNotificationEmailUseCaseImpl";
import IProcessNotificationEmailUseCase from "@Application/UseCases/ProcessNotificationEmail/IProcessNotificationEmailUseCase";
import EmailDeliveryService from "@Data/Services/EmailDeliveryService";
import EmailTemplateService from "@Data/Services/EmailTemplateService";

export function BuildProcessNotificationEmailUseCase(env: EnvironmentVariables): IProcessNotificationEmailUseCase {
  const ssmClient = new SSMClientWrapper();
  const s3Client = new S3ClientWrapper();
  const sesClient = new SESClientWrapper();

  const templateService = new EmailTemplateService(
    ssmClient,
    s3Client,
    env.TEMPLATE_BUCKET,
    env.SSM_VERIFICATION_TEMPLATE_NAME_PARAM,
    env.SSM_PASSWORD_RESET_TEMPLATE_NAME_PARAM
  );
  const emailDeliveryService = new EmailDeliveryService(sesClient, env.SES_FROM_EMAIL, env.SES_CONFIG_SET);

  return new ProcessNotificationEmailUseCaseImpl(
    templateService,
    emailDeliveryService,
    {
      subscription_thank_you: env.SSM_SUBSCRIPTION_THANK_YOU_TEMPLATE_NAME_PARAM,
      trial_ending_soon: env.SSM_TRIAL_ENDING_SOON_TEMPLATE_NAME_PARAM,
        subscription_cancelled: env.SSM_SUBSCRIPTION_CANCELLED_TEMPLATE_NAME_PARAM
    }
  );
}
