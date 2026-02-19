import {SubscriptionErrors} from "@Domain/Errors/SubscriptionErrors";
import {Result} from "neverthrow";
import Stripe from "stripe";

export default interface IProcessUpdateSubscriptionUseCase {
    
    Execute(subscriptionData : Stripe.Subscription): Promise<Result<void, SubscriptionErrors>>;
}