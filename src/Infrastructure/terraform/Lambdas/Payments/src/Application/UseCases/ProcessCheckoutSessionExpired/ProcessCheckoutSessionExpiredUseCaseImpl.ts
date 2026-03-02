import { SubscriptionErrors } from "@/Domain/Errors/SubscriptionErrors";
import IProcessCheckoutSessionExpiredUseCase
    from "@Application/UseCases/ProcessCheckoutSessionExpired/IProcessCheckoutSessionExpiredUseCase";
import {Result} from "neverthrow";
import ISubscriptionService from "@Domain/Interfaces/ISubscriptionService";
import Stripe from "stripe";

export default class ProcessCheckoutSessionExpiredUseCaseImpl implements IProcessCheckoutSessionExpiredUseCase {

    constructor(private readonly SubscriptionService : ISubscriptionService) {}

    Execute(sessionData: Stripe.Checkout.Session): Promise<Result<void, SubscriptionErrors>> {
        return this.SubscriptionService.DeleteSubscription(sessionData);
    }

}
