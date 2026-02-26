import Stripe from 'stripe';
import {SubscriptionErrors} from "@Domain/Errors/SubscriptionErrors";
import {Result} from "neverthrow";

export default interface ISubscriptionService {

    ProcessPayment(invoice: Stripe.Invoice) : Promise<Result<void, SubscriptionErrors>>;
    CancelSubscription(subscriptionData: Stripe.Subscription) : Promise<Result<void, SubscriptionErrors>>;
    UpdateSubscription(subscriptionData: Stripe.Subscription): Promise<Result<void, SubscriptionErrors>>;
    DeleteSubscription(sessionData: Stripe.Checkout.Session): Promise<Result<void, SubscriptionErrors>>;

}
