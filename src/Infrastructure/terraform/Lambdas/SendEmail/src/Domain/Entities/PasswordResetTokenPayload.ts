/**
 * JSON payload encrypted into the proprietary password reset token.
 */
export interface PasswordResetTokenPayload {
    /** User email linked to the reset operation. */
    Email: string;
    /** Decrypted Cognito code consumed by backend confirm flow. */
    CognitoCode: string;
    /** Unique token identifier for replay protection. */
    TokenId: string;
    /** Expiration timestamp in unix seconds. */
    ExpiresAtUnix: number;
}
