import {SubscriptionErrors} from "@Domain/Errors/SubscriptionErrors";
import {Result} from "neverthrow";
import Stripe from "stripe";

export default interface IProcessCheckoutSessionExpiredUseCase {
    Execute(sessionData: Stripe.Checkout.Session): Promise<Result<void, SubscriptionErrors>>;
}
