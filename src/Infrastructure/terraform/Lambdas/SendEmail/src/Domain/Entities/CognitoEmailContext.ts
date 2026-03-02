/**
 * Normalized event data used by the use case after validation and decryption.
 */
export interface CognitoEmailContext {
    /** Final recipient email address. */
    recipient: string;
    /** Friendly display name used in templates. */
    username: string;
    /** Plain code obtained after KMS decryption. */
    decryptedCode: string;
    /** Cognito trigger source used to route the flow. */
    triggerSource: string;
}
