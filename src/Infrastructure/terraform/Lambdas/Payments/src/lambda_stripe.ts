import {EventBridgeEvent, Context} from 'aws-lambda';
import Stripe from 'stripe';
import {UseCase as SuccessfulPaymentUseCase} from "@Domain/Composition/ProcessSuccessfullPaymentComposition";

import {SubscriptionErrors} from "@Domain/Errors/SubscriptionErrors";
import {Result} from "neverthrow";
import {CancelSubscriptionUseCase} from "@Domain/Composition/ProcessCancelSubscriptionComposition";
import {UpdateSubscriptionUseCase} from "@Domain/Composition/ProcessSubscriptionUpdateComposition";
import {CheckoutSessionExpiredUseCase} from "@Domain/Composition/ProcessCheckoutSessionExpiredComposition";

export interface StripeEventDetail {
    type: string;
    data: { object: any };
}

const stripe = new Stripe(process.env.STRIPE_SECRET_KEY!);


export async function stripeHandler (event: any)  {

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

      
        // 🔥 Aquí transformamos a EventBridge-like shape si quieres
        const eventBridgeLike: EventBridgeEvent<string, Stripe.Event>  = {
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

        await handler(eventBridgeLike, {} as Context);

        return {
            statusCode: 200,
            body: "OK",
        };
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
    
    const eventType = event.detail.type;
    const eventId = (event.detail as any)?.id ?? 'unknown';
    console.log('Event type:', eventType, 'event id:', eventId);
    const eventData = event.detail.data.object;
    
    try {
        switch (eventType) {
            case 'checkout.session.completed':
                const session = eventData as Stripe.Checkout.Session;

                break;
            case 'invoice.payment_failed':
            case 'invoice.paid':
                const invoicePaid: Stripe.Invoice = eventData as Stripe.Invoice;
                const invoiceResult: Result<void, SubscriptionErrors> = await SuccessfulPaymentUseCase.Execute(invoicePaid);
                if (invoiceResult.isErr()) {
                    console.log(`Error while processing payment: ${invoiceResult.error}`)
                    return {
                        statusCode: 500,
                        body: invoiceResult.error
                    };
                }
                else {
                    console.log('Payment processed successfully')
                }
                break;
            case 'customer.subscription.trial_will_end':
                console.log('Trial will end');
                // console.log(eventData);
                break;
            case 'customer.subscription.deleted':
                const subscriptionCancelled : Stripe.Subscription = eventData as Stripe.Subscription;
                let cancellationResult: Result<void, SubscriptionErrors> =  await CancelSubscriptionUseCase.Execute(subscriptionCancelled);
                if (cancellationResult.isErr()) {
                    console.log(`Error while processing subscription cancellation: ${cancellationResult.error}`)
                } else {
                    console.log('Subscription cancelled successfully')
                }
                break;
            case 'customer.subscription.updated':
                const subscriptionUpdated : Stripe.Subscription = eventData as Stripe.Subscription;
                console.log(JSON.stringify(subscriptionUpdated, null, 2));
                const updateResult : Result<void, SubscriptionErrors> = await UpdateSubscriptionUseCase.Execute(subscriptionUpdated);
                if (updateResult.isErr()) {
                    console.log(`Error while processing subscription update: ${updateResult.error}`)
                } else {
                    console.log('Subscription updated successfully')
                }
                break;
            case 'checkout.session.expired':
                const expiredSession = eventData as Stripe.Checkout.Session;
                const expiredResult: Result<void, SubscriptionErrors> = await CheckoutSessionExpiredUseCase.Execute(expiredSession);

                if (expiredResult.isErr()) {
                    console.log(`Error while processing checkout session expiration: ${expiredResult.error}`)
                } else {
                    console.log('Checkout session expired processed successfully')
                }
                break;
            default:
                console.log('Evento no manejado:', eventType);
        }

        return {
            statusCode: 200,
            body: 'Event processed'
        };
    } catch (error) {
        console.error('Error processing event:', error);
        return {
            statusCode: 500,
            body: 'Error'
        };
    }
};
