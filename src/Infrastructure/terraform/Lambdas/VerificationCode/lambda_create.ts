import {toByteArray} from 'base64-js';
import {
    buildClient,
    CommitmentPolicy,
    KmsKeyringNode,
} from '@aws-crypto/client-node';
import {Handler} from 'aws-lambda';
import {DocumentClient} from "aws-sdk/clients/dynamodb";

// Initialize the AWS Encryption SDK client with the required commitment policy
const {decrypt} = buildClient(
    CommitmentPolicy.REQUIRE_ENCRYPT_ALLOW_DECRYPT
);

// Retrieve KMS key identifiers from environment variables
const generatorKeyId: string = process.env.KEY_ALIAS!;
const keyIds: string[] = [process.env.KEY_ARN!];

// Create a KMS keyring for encryption and decryption operations
const keyring = new KmsKeyringNode({generatorKeyId, keyIds});

// Define the Lambda handler function
export const lambda_handler: Handler = async (event) => {
    // Initialize a variable to hold the decrypted code
    let plainTextCode: Uint8Array | undefined;

    console.log(event);
    
    // Check if the event contains a code to decrypt
    if (event.request.code) {
        // Decrypt the provided code using the AWS Encryption SDK
        const {plaintext} = await decrypt(
            keyring,
            toByteArray(event.request.code)
        );
        plainTextCode = plaintext;

        const dynamoDB = new DocumentClient({
            region: event.request.region,
        });
        
        const item  = {
            TableName: process.env.DYNAMO_TABLE_NAME!, // Replace with your DynamoDB table name
            Item: {
                username: event.userName,           // Replace with your item key (partition key)
                timestamp: new Date().getTime(),      // Example attribute
                verification_code: plainTextCode.toString(),  // Another attribute
            }
        };

        let request = await dynamoDB.put(item).promise();
        console.log('Item added successfully!');
    }

    // Determine the trigger source and handle accordingly
    switch (event.triggerSource) {
        case 'CustomEmailSender_SignUp':
            // Handle user sign-up event
            // Send a welcome email or verification code
            break;
        case 'CustomEmailSender_Authentication':
            // Handle user authentication event
            // Send an MFA code or notification
            break;
        case 'CustomEmailSender_ResendCode':
            // Handle resend code request
            // Resend the verification or MFA code
            break;
        case 'CustomEmailSender_ForgotPassword':
            // Handle forgot password event
            // Send password reset instructions
            break;
        case 'CustomEmailSender_UpdateUserAttribute':
            // Handle user attribute update event
            // Send confirmation for updated attributes
            break;
        case 'CustomEmailSender_VerifyUserAttribute':
            // Handle user attribute verification event
            // Send verification for user attributes
            break;
        case 'CustomEmailSender_AdminCreateUser':
            // Handle admin user creation event
            // Send initial login instructions
            break;
        case 'CustomEmailSender_AccountTakeOverNotification':
            // Handle account takeover notification
            // Alert user of suspicious activity
            break;
        default:
            // Handle unknown trigger sources
            console.warn('Unknown trigger source:', event.triggerSource);
    }

    // Return from the handler
    return;
};
