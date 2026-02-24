import {SubscriptionErrors} from "@Domain/Errors/SubscriptionErrors";
import SubscriptionEntity from "@Domain/Entities/SubscriptionEntity";
import {Result} from "neverthrow";
import ISubscriptionService from "@Domain/Interfaces/ISubscriptionService";
import Stripe from "stripe";


export default interface IProcessSuccessfulPaymentUseCase {
    
    
    Execute(subscriptionData: Stripe.Invoice): Promise<Result<void, SubscriptionErrors>>;
}