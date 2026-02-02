import {SSMClient, GetParameterCommand} from '@aws-sdk/client-ssm';

/**
 * A wrapper class for interacting with the AWS Simple Systems Manager (SSM) client.
 * This class provides methods to interact with SSM parameters.
 */
export default class SSMClientWrapper {
    
    constructor(private readonly SsmClient: SSMClient = new SSMClient()) {}

    /**
     * Retrieves the value of a parameter stored in the AWS Systems Manager Parameter Store.
     *
     * @param {string} parameterName - The name of the parameter to retrieve.
     * @return {Promise<string>} The value of the specified parameter. Returns an empty string if the parameter is not found or has no value.
     */
    public async getParameter(parameterName: string): Promise<string> {
        const command = new GetParameterCommand({ Name: parameterName });
        const response = await this.SsmClient.send(command);
        console.log(response.Parameter);
        if (!response.Parameter) {
            throw new Error(`Parameter not found in SSM parameter: ${parameterName}`);
            
        }
        return response.Parameter?.Value ?? '';
    }
}