import {Result, ResultAsync} from "neverthrow";
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
    
    /**
     * Renders the welcome email template for a specified user.
     *
     * @param {string} username - The username of the recipient for whom the welcome email is being generated.
     * @return {Promise<Result<string, EmailSenderErrors>>} A promise that resolves to a `Result` containing the rendered email template as a string, or an error of type `EmailSenderErrors` if the process fails.
     */
    RenderWelcomeEmailTemplate(username: string) : Promise<Result<string, EmailSenderErrors>>
}
