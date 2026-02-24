import { SubscriptionErrors } from "@/Domain/Errors/SubscriptionErrors";
import IProcessUpdateSubscriptionUseCase
    from "@Application/UseCases/ProcessUpdateSubscription/IProcessUpdateSubscriptionUseCase";
import {Result} from "neverthrow";
import ISubscriptionService from "@Domain/Interfaces/ISubscriptionService";
import Stripe from "stripe";

export default class ProcessUpdateSubscriptionUseCaseImpl implements IProcessUpdateSubscriptionUseCase {
    
    constructor(private readonly SubscriptionService : ISubscriptionService) {}
    
    Execute(subscriptionData : Stripe.Subscription): Promise<Result<void, SubscriptionErrors>> {
        return this.SubscriptionService.UpdateSubscription(subscriptionData);
    }
    
}