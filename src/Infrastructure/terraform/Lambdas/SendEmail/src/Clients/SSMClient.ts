import {GetParameterCommand, SSMClient} from "@aws-sdk/client-ssm";
import {errAsync, okAsync, ResultAsync} from "neverthrow";

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
        return ResultAsync.fromPromise(
            this.ssmClient.send(command),
            _error => {
                console.error("Failed to retrieve SSM parameter:", parameterName, _error);
                return "SSM_PARAMETER_NOT_FOUND"
            }
        ).andThen(response => {
            const value = response.Parameter?.Value;
            return value ? okAsync(value) : errAsync("SSM_PARAMETER_NOT_FOUND");
        });
    }
}
