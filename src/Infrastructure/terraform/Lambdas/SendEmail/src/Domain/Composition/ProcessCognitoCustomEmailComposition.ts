import IProcessCognitoCustomEmailUseCase from "@Application/UseCases/ProcessCognitoCustomEmail/IProcessCognitoCustomEmailUseCase";
import ProcessCognitoCustomEmailUseCaseImpl
    from "@Application/UseCases/ProcessCognitoCustomEmail/ProcessCognitoCustomEmailUseCaseImpl";
import SESClientWrapper from "@/Clients/SESClient";
import S3ClientWrapper from "@/Clients/S3Client";
import SSMClientWrapper from "@/Clients/SSMClient";
import {EnvironmentVariables} from "@/Interfaces/IEnvironmentVariables";
import CognitoCodeCipherService from "@Data/Services/CognitoCodeCipherService";
import EmailDeliveryService from "@Data/Services/EmailDeliveryService";
import EmailTemplateService from "@Data/Services/EmailTemplateService";
import PasswordResetTokenService from "@Data/Services/PasswordResetTokenService";

/**
 * Composes dependencies for Cognito custom email processing use case.
 *
 * @param env Lambda environment configuration.
 * @returns Fully wired use case instance.
 */
export function BuildProcessCognitoCustomEmailUseCase(env: EnvironmentVariables): IProcessCognitoCustomEmailUseCase {
    const ssmClient = new SSMClientWrapper();
    const s3Client = new S3ClientWrapper();
    const sesClient = new SESClientWrapper();

    const codeCipherService = new CognitoCodeCipherService(env.KEY_ALIAS, env.KEY_ARN);
    const templateService = new EmailTemplateService(
        ssmClient,
        s3Client,
        env.TEMPLATE_BUCKET,
        env.SSM_VERIFICATION_TEMPLATE_NAME_PARAM,
        env.SSM_PASSWORD_RESET_TEMPLATE_NAME_PARAM
    );
    const emailDeliveryService = new EmailDeliveryService(sesClient, env.SES_FROM_EMAIL, env.SES_CONFIG_SET);
    const passwordResetTokenService = new PasswordResetTokenService(
        env.PASSWORD_RESET_TOKEN_SECRET,
        env.PASSWORD_RESET_FRONTEND_URL,
        env.PASSWORD_RESET_TOKEN_TTL_MINUTES
    );

    return new ProcessCognitoCustomEmailUseCaseImpl(
        codeCipherService,
        templateService,
        emailDeliveryService,
        passwordResetTokenService
    );
}
