import {EventBridgeEvent, Context} from 'aws-lambda';
import Stripe from 'stripe';
import {UseCase as SuccessfulPaymentUseCase} from "@Domain/Composition/ProcessSuccessfullPaymentComposition";
import {SubscriptionErrors} from "@Domain/Errors/SubscriptionErrors";
import {Result} from "neverthrow";

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
            case 'invoice.paid':
                const invoicePaid : Stripe.Invoice = eventData as Stripe.Invoice;
                const result : Result<void, SubscriptionErrors> = await SuccessfulPaymentUseCase.Execute(invoicePaid);
                if (result.isErr()) {
                    console.log(`Error while processing payment: ${result.error}`)
                }
                break;
            case 'invoice.payment_failed':
                console.log('Invoice payment failed:', eventData);
                break;
            case 'customer.subscription.deleted':
                console.log('Subscription deleted:', eventData);
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