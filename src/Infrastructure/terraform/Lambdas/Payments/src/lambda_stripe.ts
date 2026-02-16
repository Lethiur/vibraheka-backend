import { EventBridgeEvent, Context } from 'aws-lambda';
import Stripe from 'stripe';
import DynamoDBClient from "./Utils/DynamoDBClient";
import SubscriptionEntity from "./Entities/SubscriptionEntity";

const stripe = new Stripe(process.env.STRIPE_SECRET_KEY!, {
    apiVersion: '2026-01-28.clover'
});

interface StripeEventDetail {
    type: string;
    data: { object: any };
}

// Lambda handler
export const handler = async (event: EventBridgeEvent<string, StripeEventDetail>, context: Context) => {
    
    const dynamoDBClient : DynamoDBClient<SubscriptionEntity> = new DynamoDBClient<SubscriptionEntity>(process.env.DYNAMO_TABLE_NAME!);
    
    console.log('Received EventBridge event:', JSON.stringify(event, null, 2));

    const eventType = event.detail.type;
    const eventData = event.detail.data.object;

    try {
        switch (eventType) {
            case 'checkout.session.completed':
                const session = eventData as Stripe.Checkout.Session;
                console.log('Checkout session completed:', session);
                // TODO: confirmar suscripción interna en tu DB
                break;

            case 'invoice.paid':
                const invoicePaid = eventData as Stripe.Invoice;
                const remaining = invoicePaid.amount_paid - invoicePaid.total
                if (remaining == 0) {
                    console.log('Invoice fully paid:', invoicePaid);
                }
                
                if (typeof invoicePaid.customer === 'string') {
                    console.log('Customer ID:', invoicePaid.customer);
                    const customerID : string = invoicePaid.customer as string;
                    
                    const entity : SubscriptionEntity[] = await dynamoDBClient.queryIndexWithoutFilter('ExternalCustomer-Index', `ExternalCustomerID = :customerID`, {":customerID": customerID});
                    
                    if (entity.length > 0) {
                        const subscriptionEntity : SubscriptionEntity = entity[0];
                        subscriptionEntity.Status = "ACTIVE";
                        await dynamoDBClient.putItem(subscriptionEntity);
                    }
                }
                
                
                console.log('Invoice paid:', invoicePaid);
                // TODO: marcar renovación como exitosa en tu DB
                break;

            case 'invoice.payment_failed':
                console.log('Invoice payment failed:', eventData);
                // TODO: notificar fallo y actualizar estado en tu DB
                break;

            case 'customer.subscription.deleted':
                console.log('Subscription deleted:', eventData);
                // TODO: marcar suscripción como cancelada en tu DB
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
