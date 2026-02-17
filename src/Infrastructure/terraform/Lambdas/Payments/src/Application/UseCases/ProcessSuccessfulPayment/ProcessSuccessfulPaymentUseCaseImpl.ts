import { SubscriptionErrors } from "@/Domain/Errors/SubscriptionErrors";
import IProcessSuccessfulPaymentUseCase
    from "@Application/UseCases/ProcessSuccessfulPayment/IProcessSuccessfulPaymentUseCase";
import {Result} from "neverthrow";
import ISubscriptionService from "@Domain/Interfaces/ISubscriptionService";
import Stripe from "stripe";


export default class ProcessSuccessfulPaymentUseCaseImpl implements IProcessSuccessfulPaymentUseCase {
    
    constructor(private readonly SubscriptionService : ISubscriptionService) {}
    
    public Execute(subscriptionData: Stripe.Invoice): Promise<Result<void, SubscriptionErrors>> {
        return this.SubscriptionService.UpdateSubscriptionBasedOnPaymentDetails(subscriptionData);
    }
}