import Stripe from 'stripe';
import {SubscriptionErrors} from "@Domain/Errors/SubscriptionErrors";
import {Result} from "neverthrow";

export default interface ISubscriptionService {

    UpdateSubscriptionBasedOnPaymentDetails(invoice: Stripe.Invoice) : Promise<Result<void, SubscriptionErrors>>;
    
}

