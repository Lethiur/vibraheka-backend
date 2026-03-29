import {EnvironmentVariables} from "../Interfaces/IEnvironmentVariables";

export function requireEnv<K extends keyof NodeJS.ProcessEnv>(key: string): string {

    const value: string | undefined = process.env[key];
    
    if (!value) {
        throw new Error(`Missing required environment variable: ${String(key)}`);
    }

    return value;
}
