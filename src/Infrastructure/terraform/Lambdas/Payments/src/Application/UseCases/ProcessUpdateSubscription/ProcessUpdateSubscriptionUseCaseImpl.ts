import {SubscriptionErrors} from "@/Domain/Errors/SubscriptionErrors";
import IProcessUpdateSubscriptionUseCase
    from "@Application/UseCases/ProcessUpdateSubscription/IProcessUpdateSubscriptionUseCase";
import {err, Result} from "neverthrow";
import ISubscriptionService from "@Domain/Interfaces/ISubscriptionService";
import Stripe from "stripe";
import INotificationService from "@Domain/Interfaces/INotificationService";
import SubscriptionEntity from "@Domain/Entities/SubscriptionEntity";
import NotificationEmailEventDetail from "@Domain/Events/NotificationEmailEvent";

export default class ProcessUpdateSubscriptionUseCaseImpl implements IProcessUpdateSubscriptionUseCase {

    private readonly StripeClient: Stripe;

    constructor(private readonly SubscriptionService: ISubscriptionService, private readonly NotificationService: INotificationService) {
        this.StripeClient = new Stripe(process.env.STRIPE_SECRET_KEY!);
    }

    public async Execute(subscriptionData: Stripe.Subscription): Promise<Result<void, SubscriptionErrors>> {
        const result: Result<SubscriptionEntity, SubscriptionErrors> = await this.SubscriptionService.UpdateSubscription(subscriptionData);
        if (result.isOk()) {
            const subscription: SubscriptionEntity = result.value;
            const customer = await this.StripeClient.customers.retrieve(subscription.ExternalCustomerID);

            if (customer.deleted) {
                return err(SubscriptionErrors.CUSTOMER_DISABLED);
            }

            const email: string | null = customer.email;

            if (email == null) {
                console.log("Customer has no email");
                return err(SubscriptionErrors.CUSTOMER_NOT_FOUND);
            }
            let notificationEvent: NotificationEmailEventDetail = {
                username: customer.name!,
                attachments: [],
                recipient: email,
                templateType: "trial_ending_soon",
                subject: "Tu periodo de prueba acabara pronto",
                templateData: {},
            }
            if (subscription.SubscriptionStatus == 'Trialing') {
                notificationEvent.templateType = 'subscription_reactivated';
            } else {
                notificationEvent.templateType = 'subscription_cancelled';
            }

            return (await this.NotificationService.Publish(notificationEvent))
                .map(_ => undefined)
                .mapErr(error => {
                    console.log("Error publishing notification: ", error);
                    return SubscriptionErrors.EMAIL_NOTIFICATION_ERROR;
                });

        }

        return result.map(_ => undefined);

    }

}