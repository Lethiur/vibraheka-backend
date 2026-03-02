import {buildClient, CommitmentPolicy, KmsKeyringNode} from "@aws-crypto/client-node";
import ICognitoCodeCipherService from "@Domain/Interfaces/ICognitoCodeCipherService";
import {ResultAsync} from "neverthrow";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";

const {decrypt} = buildClient(CommitmentPolicy.REQUIRE_ENCRYPT_ALLOW_DECRYPT);

/**
 * Decrypts Cognito encrypted codes using the configured KMS keyring.
 */
export default class CognitoCodeCipherService implements ICognitoCodeCipherService {
    constructor(
        private readonly keyAlias: string,
        private readonly keyArn: string
    ) {
    }

    /**
     * Decrypts an encrypted Cognito code.
     *
     * @param encryptedCode Base64 value provided by Cognito trigger event.
     * @returns Async result with plaintext code or domain error.
     */
    public DecryptCode(encryptedCode: string): ResultAsync<string, EmailSenderErrors> {
        return ResultAsync.fromPromise((async () => {
            try {
                console.log("Decrypting Cognito code payload");
                const keyring = new KmsKeyringNode({
                    generatorKeyId: this.keyAlias,
                    keyIds: [this.keyArn]
                });

                const cipherBytes = Buffer.from(encryptedCode, "base64");
                const {plaintext} = await decrypt(keyring, cipherBytes);
                return Buffer.from(plaintext).toString("utf-8");
            } catch (error) {
                console.error("Failed to decrypt Cognito code payload", {error});
                throw error;
            }
        })(), () => EmailSenderErrors.DECRYPTION_FAILED);
    }
}
