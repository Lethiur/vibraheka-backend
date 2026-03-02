import {ResultAsync} from "neverthrow";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";

/**
 * Contract for decrypting Cognito encrypted codes.
 */
export default interface ICognitoCodeCipherService {
    /**
     * Decrypts the encrypted code sent by Cognito.
     *
     * @param encryptedCode Base64-encoded encrypted code from Cognito.
     * @returns Async result containing the plaintext code or a domain error.
     */
    DecryptCode(encryptedCode: string): ResultAsync<string, EmailSenderErrors>;
}
