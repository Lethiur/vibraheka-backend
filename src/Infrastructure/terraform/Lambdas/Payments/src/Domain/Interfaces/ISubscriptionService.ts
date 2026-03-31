import Stripe from 'stripe';
import {SubscriptionErrors} from "@Domain/Errors/SubscriptionErrors";
import {Result} from "neverthrow";
import SubscriptionEntity from "@Domain/Entities/SubscriptionEntity";

export default interface ISubscriptionService {

    ProcessPayment(invoice: Stripe.Invoice) : Promise<Result<SubscriptionEntity, SubscriptionErrors>>;
    CancelSubscription(subscriptionData: Stripe.Subscription) : Promise<Result<void, SubscriptionErrors>>;
    UpdateSubscription(subscriptionData: Stripe.Subscription): Promise<Result<SubscriptionEntity, SubscriptionErrors>>;
    DeleteSubscription(sessionData: Stripe.Checkout.Session): Promise<Result<void, SubscriptionErrors>>;

}
