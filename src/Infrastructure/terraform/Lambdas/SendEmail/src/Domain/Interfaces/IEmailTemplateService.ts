import {ResultAsync} from "neverthrow";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";

/**
 * Contract for resolving and rendering email templates.
 */
export default interface IEmailTemplateService {
    /**
     * Renders the verification template.
     *
     * @param username Receiver display name.
     * @param code Plain verification code.
     * @returns Async result containing rendered HTML or a domain error.
     */
    RenderVerificationTemplate(username: string, code: string): ResultAsync<string, EmailSenderErrors>;

    /**
     * Renders the password reset template.
     *
     * @param username Receiver display name.
     * @param token Proprietary reset token.
     * @param resetLink Complete link used by the frontend reset screen.
     * @returns Async result containing rendered HTML or a domain error.
     */
    RenderPasswordResetTemplate(username: string, token: string, resetLink: string): ResultAsync<string, EmailSenderErrors>;
}
