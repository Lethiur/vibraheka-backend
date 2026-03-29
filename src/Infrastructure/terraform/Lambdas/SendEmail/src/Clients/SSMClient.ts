import {GetParameterCommand, SSMClient} from "@aws-sdk/client-ssm";
import {err, errAsync, okAsync, ResultAsync} from "neverthrow";

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
    public getParameter(parameterName: string): ResultAsync<string, string> {
        const command = new GetParameterCommand({Name: parameterName});
        return ResultAsync.fromPromise(this.ssmClient.send(command), error => {
            console.log("Failed to retrieve the SSM parameter from store")
            throw new Error("SSM_PARAMETER_NOT_FOUND")
        }).map(response => {
            if (!response.Parameter?.Value) {
                throw new Error("SSM_PARAMETER_NOT_FOUND");
            }
            return response.Parameter.Value
        });
    }
}
