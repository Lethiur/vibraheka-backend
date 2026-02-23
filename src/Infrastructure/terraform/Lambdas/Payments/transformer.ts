import * as fs from 'fs';

// Ajustamos a la interfaz que usa tu Lambda
interface StripeEventDetail {
    type: string;
    data: { object: any };
}

interface SimulatedEventBridgeEvent {
    version: string;
    id: string;
    'detail-type': string;
    source: string;
    account: string;
    time: string;
    region: string;
    resources: string[];
    detail: StripeEventDetail; // Aquí inyectamos el evento de Stripe
}

const transformForLambda = (inputFile: string, outputFile: string) => {
    try {
        const rawData = fs.readFileSync(inputFile, 'utf8');
        const stripeExport = JSON.parse(rawData);

        // Mapeamos cada evento de Stripe al formato EventBridgeEvent
        const simulatedEvents: SimulatedEventBridgeEvent[] = stripeExport.data.map((stripeEvent: any) => {
            return {
                version: "0",
                id: `sim-${Math.random().toString(36).substr(2, 9)}`,
                'detail-type': stripeEvent.type,
                source: 'aws.partner/stripe.com',
                account: '123456789012',
                time: new Date(stripeEvent.created * 1000).toISOString(),
                region: 'eu-west-1',
                resources: [],
                detail: {
                    type: stripeEvent.type,
                    data: {
                        object: stripeEvent.data.object // Extraemos el 'object' que espera tu Lambda
                    }
                }
            };
        });

        fs.writeFileSync(outputFile, JSON.stringify(simulatedEvents, null, 2));
        console.log(`✅ ¡Listo! Se han generado ${simulatedEvents.length} eventos simulados.`);
    } catch (error) {
        console.error("❌ Error:", error);
    }
};

transformForLambda('actualizaciones.json', 'customer.subscription.updated.json');