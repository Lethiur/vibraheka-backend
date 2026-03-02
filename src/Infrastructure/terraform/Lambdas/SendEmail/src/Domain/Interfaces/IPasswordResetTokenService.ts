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
     * @returns Result containing the link or a domain error.
     */
    BuildPasswordResetLink(token: string): Result<string, EmailSenderErrors>;
}
