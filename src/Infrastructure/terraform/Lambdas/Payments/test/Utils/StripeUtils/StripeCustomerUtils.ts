import Stripe from "stripe";

export async function CreateTestCustomer(stripeClient: Stripe, withPaymentMethod: boolean = false) : Promise<{ClockID: string, CustomerID: string}> {
    const testClock = await stripeClient.testHelpers.testClocks.create({
        // Momento inicial del reloj (timestamp en segundos)
        frozen_time: Math.floor(Date.now() / 1000),
    });
    
  
    
    const customer = await stripeClient.customers.create({
        name: 'Test Customer',
        email: `${Math.random().toString(36).substring(2, 15)}@example.com`,
        phone: '+1234567890',
        test_clock: testClock.id,
        metadata: {
            userId: 'test-user-id',
        },
    });

    if (withPaymentMethod) {
        const pm = await stripeClient.paymentMethods.create({
            type: "card",
            card: { token: "tok_visa" }, // token de test
        });
        await stripeClient.paymentMethods.attach(pm.id, {
            customer: customer.id,
        });

        await stripeClient.customers.update(customer.id, {
            invoice_settings: {
                default_payment_method: pm.id,
            },
        });
    }

    return {CustomerID: customer.id, ClockID: testClock.id};
}