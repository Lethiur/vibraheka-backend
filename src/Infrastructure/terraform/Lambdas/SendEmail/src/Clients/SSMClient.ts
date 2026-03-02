import {GetParameterCommand, SSMClient} from "@aws-sdk/client-ssm";

/**
 * Thin wrapper around AWS SSM client used to resolve parameter values.
 */
export default class SSMClientWrapper {
    constructor(private readonly ssmClient: SSMClient = new SSMClient()) {}

    /**
     * Reads one SSM parameter value.
     *
     * @param parameterName Full SSM parameter name.
     * @returns Parameter value.
     * @throws Error when parameter does not exist.
     */
    public async getParameter(parameterName: string): Promise<string> {
        const command = new GetParameterCommand({Name: parameterName});
        const response = await this.ssmClient.send(command);

        if (!response.Parameter) {
            throw new Error(`Parameter not found in SSM parameter: ${parameterName}`);
        }

        return response.Parameter.Value ?? "";
    }
}
