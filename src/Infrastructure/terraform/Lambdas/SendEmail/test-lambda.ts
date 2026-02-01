import dotenv from 'dotenv';

dotenv.config();
import {handler} from "./src/lambda_send_email";
import { APIGatewayProxyEvent, Context } from 'aws-lambda';

const mockEvent: APIGatewayProxyEvent = {
    body: JSON.stringify({ name: 'Señor Pelotas' }),
    headers: {},
    multiValueHeaders: {},
    httpMethod: 'POST',
    isBase64Encoded: false,
    path: '/test',
    pathParameters: null,
    queryStringParameters: null,
    multiValueQueryStringParameters: null,
    stageVariables: null,
    requestContext: {} as any,
    resource: ''
};

async function testLambda() {
    await handler({
        request: {
            codeParameter: "2342",
            usernameParameter: "mtesqtsdlc2@gmail.com",
            userAttributes: {},
            linkParameter: ""
        },
        response: {
            smsMessage: null,
            emailMessage: null,
            emailSubject: null
        },
        version: "",
        region: "",
        userPoolId: "",
        triggerSource: "CustomMessage_SignUp",
        userName: "",
        callerContext: {
            awsSdkVersion: "",
            clientId: ""
        }
    }, {} as Context, (error) => {})
}

testLambda();