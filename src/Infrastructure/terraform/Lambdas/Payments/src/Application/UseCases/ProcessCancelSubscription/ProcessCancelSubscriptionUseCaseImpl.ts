import { SubscriptionErrors } from "@/Domain/Errors/SubscriptionErrors";
import IProcessCancelSubscriptionUseCase from "@Application/UseCases/ProcessCancelSubscription/IProcessCancelSubscriptionUseCase";
import {Result} from "neverthrow";
import Stripe from "stripe";
import ISubscriptionService from "@Domain/Interfaces/ISubscriptionService";

export default class ProcessCancelSubscriptionUseCaseImpl implements IProcessCancelSubscriptionUseCase {
    
    constructor(private readonly SubscriptionService : ISubscriptionService) {}
    
    Execute(subscriptionData: Stripe.Subscription): Promise<Result<void, SubscriptionErrors>> {
        return this.SubscriptionService.CancelSubscription(subscriptionData);
    }
    
}