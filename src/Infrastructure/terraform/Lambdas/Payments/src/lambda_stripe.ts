import {EventBridgeEvent, Context} from 'aws-lambda';
import Stripe from 'stripe';
import {UseCase as SuccessfulPaymentUseCase} from "@Domain/Composition/ProcessSuccessfullPaymentComposition";

import {SubscriptionErrors} from "@Domain/Errors/SubscriptionErrors";
import {Result} from "neverthrow";
import {CancelSubscriptionUseCase} from "@Domain/Composition/ProcessCancelSubscriptionComposition";
import {UpdateSubscriptionUseCase} from "@Domain/Composition/ProcessSubscriptionUpdateComposition";

interface StripeEventDetail {
    type: string;
    data: { object: any };
}

// Lambda handler
export const handler = async (event: EventBridgeEvent<string, StripeEventDetail>, context: Context) => {

    const eventType = event.detail.type;
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
                }
                else {
                    console.log('Payment processed successfully')
                }
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
                const updateResult : Result<void, SubscriptionErrors> = await UpdateSubscriptionUseCase.Execute(subscriptionUpdated);
                if (updateResult.isErr()) {
                    console.log(`Error while processing subscription update: ${updateResult.error}`)
                } else {
                    console.log('Subscription updated successfully')
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