import { SubscriptionErrors } from "@/Domain/Errors/SubscriptionErrors";
import ITrialWillEndUseCase from "@Application/UseCases/ProcessTrialWillEnd/ITrialWillEndUseCase";
import {err, Result} from "neverthrow";
import ISubscriptionService from "@Domain/Interfaces/ISubscriptionService";
import Stripe from "stripe";
import INotificationService from "@Domain/Interfaces/INotificationService";
import NotificationEmailEventDetail from "@Domain/Events/NotificationEmailEvent";

export default class TrialWillEndUseCaseImpl implements ITrialWillEndUseCase {
    
    private readonly StripeClient : Stripe;
    
    constructor(private readonly NotificationService : INotificationService) {
        this.StripeClient = new Stripe(process.env.STRIPE_SECRET_KEY!);
    }
    
    async Execute(customerId : string): Promise<Result<void, SubscriptionErrors>> {
        const customer = await this.StripeClient.customers.retrieve(customerId);

        if (customer.deleted) {
            return err(SubscriptionErrors.CUSTOMER_DISABLED);
        }

        const email : string | null = customer.email;
        
        if (email == null) {
            console.log("Customer has no email");
            return err(SubscriptionErrors.CUSTOMER_NOT_FOUND);
        }
        
        const notificationEvent : NotificationEmailEventDetail = {
            attachments: [],
            recipient: email,
            templateType: "trial_ending_soon",
            subject: "Tu periodo de prueba acabara pronto",
            templateData: {},
        }
        
        const result = await this.NotificationService.Publish(notificationEvent);
        
        return result.mapErr(error => {
            console.log("Error while sending the email event", error);
            return SubscriptionErrors.EMAIL_NOTIFICATION_ERROR;
        })
    }
    
}