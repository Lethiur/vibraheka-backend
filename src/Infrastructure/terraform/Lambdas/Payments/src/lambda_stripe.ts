import {EventBridgeEvent, Context} from 'aws-lambda';
import Stripe from 'stripe';
import {UseCase as SuccessfulPaymentUseCase} from "@Domain/Composition/ProcessSuccessfullPaymentComposition";

import {SubscriptionErrors} from "@Domain/Errors/SubscriptionErrors";
import {err, ok, Result} from "neverthrow";
import {CancelSubscriptionUseCase} from "@Domain/Composition/ProcessCancelSubscriptionComposition";
import {UpdateSubscriptionUseCase} from "@Domain/Composition/ProcessSubscriptionUpdateComposition";
import {CheckoutSessionExpiredUseCase} from "@Domain/Composition/ProcessCheckoutSessionExpiredComposition";
import {ProcessTrialWillEndUseCase} from "@Domain/Composition/ProcessTrialWillEndComposition";
import NotificationService from "@Data/Services/NotificationService";


export interface StripeEventDetail {
    type: string;
    data: { object: any };
}

const stripe = new Stripe(process.env.STRIPE_SECRET_KEY!);

export async function stripeHandler(event: any) {

    try {
        const signature =
            event.headers["stripe-signature"] ||
            event.headers["Stripe-Signature"];

        const rawBody =
            typeof event.body === "string"
                ? event.body
                : JSON.stringify(event.body);
        const stripeEvent = stripe.webhooks.constructEvent(
            rawBody,
            signature,
            process.env.STRIPE_WEBHOOK_SECRET!
        );

        const eventBridgeLike: EventBridgeEvent<string, Stripe.Event> = {
            version: "0",
            id: stripeEvent.id,
            "detail-type": stripeEvent.type,
            source: "stripe",
            account: "local",
            time: new Date().toISOString(),
            region: "eu-west-1",
            resources: [],
            detail: stripeEvent,
        };

        return await handler(eventBridgeLike as unknown as EventBridgeEvent<string, StripeEventDetail>, {} as Context);
    } catch (err) {
        console.error("Webhook error:", err);
        return {
            statusCode: 400,
            body: "Invalid signature",
        };
    }


}

// Lambda handler
export const handler = async (event: EventBridgeEvent<string, StripeEventDetail>, context: Context) => {

    const eventType: string = event.detail.type;
    const eventData: any = event.detail.data.object;

    try {
        let result: Result<void, SubscriptionErrors>;
        switch (eventType) {
            case 'checkout.session.completed':
                result = ok(undefined);
                break;
            case 'invoice.payment_failed':
            case 'invoice.paid':
                result = await SuccessfulPaymentUseCase.Execute(eventData as Stripe.Invoice);
                break;
            case 'customer.subscription.trial_will_end':
                result = await ProcessTrialWillEndUseCase.Execute(eventData.customer);
                break;
            case 'customer.subscription.deleted':
                result = await CancelSubscriptionUseCase.Execute(eventData as Stripe.Subscription);
                break;
            case 'customer.subscription.updated':
                result = await UpdateSubscriptionUseCase.Execute(eventData as Stripe.Subscription);
                break;
            case 'checkout.session.expired':
                result = await CheckoutSessionExpiredUseCase.Execute(eventData as Stripe.Checkout.Session);
                break;
            default:
                result = ok(undefined);
                console.log('Unhandled event, returning ok to avoid retries', eventType);
        }

        return result.match(_ => {
            return {
                statusCode: 200,
                body: 'OK'
            };
        }, error => {
            return {
                statusCode: 500,
                body: error
            };
        })
    } catch (error) {
        console.error('Error processing event:', error);
        return {
            statusCode: 500,
            body: (error as Error).message || JSON.stringify(error, null, 2)
        };
    }
};
