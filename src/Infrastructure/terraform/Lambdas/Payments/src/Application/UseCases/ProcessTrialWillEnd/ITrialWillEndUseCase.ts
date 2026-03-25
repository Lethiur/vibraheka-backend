import {SubscriptionErrors} from "@Domain/Errors/SubscriptionErrors";
import {Result} from "neverthrow";

export default interface ITrialWillEndUseCase {
    Execute(customerId : string): Promise<Result<void, SubscriptionErrors>>;
}