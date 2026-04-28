import {Result} from "neverthrow";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";

/**
 * Contract for creating reset tokens and reset links.
 */
export default interface IPasswordResetTokenService {
    /**
     * Builds the proprietary token consumed later by the backend.
     *
     * @param email User email.
     * @param cognitoCode Decrypted Cognito forgot-password code.
     * @returns Result containing token string or a domain error.
     */
    BuildPasswordResetToken(email: string, cognitoCode: string): Result<string, EmailSenderErrors>;

    /**
     * Builds the final link sent to the user.
     *
     * @param token Proprietary reset token.
     * @param frontendResetUrl
     * @returns Result containing the link or a domain error.
     */
    BuildPasswordResetLink(token: string, frontendResetUrl : string): Result<string, EmailSenderErrors>;

    /**
     * Generates a verification link for user email or password reset purposes.
     *
     * @param {string} token - The unique token associated with the verification process.
     * @param {string} frontendResetUrl - The base URL of the frontend application for constructing the full verification link.
     * @return {Result<string, EmailSenderErrors>} A result object containing either the generated verification link as a string or an error related to email sending.
     */
    BuildVerificationLink(token: string, frontendResetUrl : string): Result<string, EmailSenderErrors>;
}
