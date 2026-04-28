import {createCipheriv, createHash, randomBytes, randomUUID} from "crypto";
import {err, ok, Result} from "neverthrow";

const TOKEN_PREFIX = "v1";
const NONCE_BYTES = 12;
const TAG_BYTES = 16;

/**
 * Creates proprietary password-reset tokens consumed by backend replay-protection flow.
 */
export default class PasswordResetTokenService {
    private readonly encryptionKey: Buffer;
    private readonly secretFingerprint: string;

    constructor(
        tokenSecret: string,
        private readonly tokenTtlMinutes: number
    ) {
        const normalizedSecret = tokenSecret.trim();
        this.encryptionKey = createHash("sha256")
            .update(normalizedSecret, "utf-8")
            .digest();
        this.secretFingerprint = this.encryptionKey.toString("hex").slice(0, 12);
        console.log("Password reset token service initialized", {
            secretFingerprint: this.secretFingerprint
        });
    }

    /**
     * Builds an encrypted token payload for reset confirmation.
     *
     * @param email User email.
     * @param cognitoCode Plain Cognito forgot-password code.
     * @returns Result with encoded token or domain error.
     */
    public BuildPasswordResetToken(email: string, cognitoCode: string): Result<string, string> {
        try {
            console.log("Building password reset token payload", {email});
            const expiresAtUnix = Math.floor(Date.now() / 1000) + this.tokenTtlMinutes * 60;
            const payload = {
                Email: email,
                CognitoCode: cognitoCode,
                TokenId: randomUUID(),
                ExpiresAtUnix: expiresAtUnix
            };

            const plainBytes = Buffer.from(JSON.stringify(payload), "utf-8");
            const nonce = randomBytes(NONCE_BYTES);
            const cipher = createCipheriv("aes-256-gcm", this.encryptionKey, nonce, {authTagLength: TAG_BYTES});
            const encrypted = Buffer.concat([cipher.update(plainBytes), cipher.final()]);
            const tag = cipher.getAuthTag();
            const payloadBytes = Buffer.concat([nonce, tag, encrypted]);
            const encoded = payloadBytes.toString("base64url");
            return ok(`${TOKEN_PREFIX}.${encoded}`);
        } catch (error) {
            console.error("Failed building password reset token", {email, error});
            return err("an error occurred while building the password reset token");
        }
    }
}
