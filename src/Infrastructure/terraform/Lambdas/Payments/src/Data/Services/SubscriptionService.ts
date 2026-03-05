import {SubscriptionErrors} from "@/Domain/Errors/SubscriptionErrors";
import ISubscriptionService from "@Domain/Interfaces/ISubscriptionService";
import {err, ok, Result} from "neverthrow";
import Stripe from "stripe";
import ISubscriptionRepository from "@Domain/Interfaces/ISubscriptionRepository";
import SubscriptionEntity from "@Domain/Entities/SubscriptionEntity";

/**
 * A service class responsible for handling subscription-related operations.
 * Implements the ISubscriptionService interface.
 */
export default class SubscriptionService implements ISubscriptionService {

    
    private readonly StripeClient : Stripe;
    
    /**
     * Constructs an instance of the class with the provided subscription repository.
     *
     * @param {ISubscriptionRepository} Repository - The subscription repository used for data operations.
     */
    constructor(private readonly Repository: ISubscriptionRepository) {
        this.StripeClient = new Stripe(process.env.STRIPE_SECRET_KEY!);
    }

    public async DeleteSubscription(sessionData: Stripe.Checkout.Session): Promise<Result<void, SubscriptionErrors>> {
        const customerID = sessionData.customer;

        if (typeof customerID !== 'string' || !customerID) {
            console.log(`The stripe customer is invalid or not expanded: ${customerID}`);
            return err(SubscriptionErrors.CUSTOMER_NOT_FOUND);
        }

        const repositoryResult: Result<SubscriptionEntity, SubscriptionErrors> = await this.Repository.GetSubscriptionForCustomer(customerID);

        if (repositoryResult.isErr()) {
            return err(repositoryResult.error);
        }

        return this.Repository.DeleteSubscription(repositoryResult.value);
    }

    public async CancelSubscription(subscriptionData: Stripe.Subscription): Promise<Result<void, SubscriptionErrors>> {
        const customerID = subscriptionData.customer as string;
        const stripeSubscriptionID = subscriptionData.id as string;
        const repositoryResult: Result<SubscriptionEntity, SubscriptionErrors> = await this.Repository.GetSubscriptionForCustomer(customerID);

        if (repositoryResult.isErr()) {
            return err(repositoryResult.error);
        }

        const subscriptionEntity: SubscriptionEntity = repositoryResult.value;

        if (stripeSubscriptionID === subscriptionEntity.ExternalSubscriptionID) {
            subscriptionEntity.SubscriptionStatus = 'Cancelled';
            subscriptionEntity.Status = 'PaymentFailed';
            subscriptionEntity.EndDate = new Date(subscriptionData.cancel_at! * 1000).toISOString();
            return (await this.Repository.SaveSubscription(subscriptionEntity)).map(_ => void (0));
        }

        return err(SubscriptionErrors.WRONG_PAYMENT_FOR_SUBSCRIPTION);
    }

    public async UpdateSubscription(subscriptionData: Stripe.Subscription): Promise<Result<void, SubscriptionErrors>> {
        console.log("Actualizando subscripcion!");
        const customerID = subscriptionData.customer as string;
        const stripeSubscriptionID = subscriptionData.id as string;
        const repositoryResult: Result<SubscriptionEntity, SubscriptionErrors> = await this.Repository.GetSubscriptionForCustomer(customerID);

        if (repositoryResult.isErr()) {
            if (repositoryResult.error === SubscriptionErrors.SUBSCRIPTION_NOT_FOUND) {
                console.log(`Subscription for customer ${customerID} not found`)
            }
            return err(repositoryResult.error);
        }

        const subscriptionEntity: SubscriptionEntity = repositoryResult.value;

        if (stripeSubscriptionID === subscriptionEntity.ExternalSubscriptionID) {
            console.log("Received event for update subscription with date", subscriptionData.cancel_at, " and status ", subscriptionData.status);

            if (subscriptionData.pending_setup_intent != null) {
                console.log("Subscription has pending setup intent, setting the order to payment pending");
                subscriptionEntity.Status = 'PaymentPending';
                subscriptionEntity.SubscriptionStatus = 'Trialing';
            } else if (["past_due", "unpaid","incomplete", "incomplete_expired"].includes(subscriptionData.status)) {
                console.log("Suscripcion actualizada pero no corresponde a accion del usuario, descartando");
            } else {
                
                if (subscriptionData.cancel_at) {
                    console.log("Cancel at is set, setting for cancellation");
                    subscriptionEntity.SubscriptionStatus = 'ToBeCancelled';
                } else {
                    if (subscriptionEntity.Status === 'OrderDelayed') {
                        console.log("Subscription is in trial mode, setting to trialing");
                        subscriptionEntity.SubscriptionStatus = 'Trialing';
                    } else {
                        console.log("Subscription is not in trial mode, setting to active");
                        subscriptionEntity.SubscriptionStatus = 'Active';
                    }
                }
            }
            
            return (await this.Repository.SaveSubscription(subscriptionEntity)).map(_ => void (0));
        }

        return err(SubscriptionErrors.WRONG_PAYMENT_FOR_SUBSCRIPTION);
    }

    /**
     * Updates a subscription based on the provided payment details from a Stripe invoice.
     *
     * @param {Stripe.Invoice} invoice - The Stripe invoice containing payment details used to update the subscription.
     * @return {Promise<Result<void, SubscriptionErrors>>} A promise that resolves to a result object.
     * On success, the result contains void. On failure, it contains subscription-related errors.
     */
    public async ProcessPayment(invoice: Stripe.Invoice): Promise<Result<void, SubscriptionErrors>> {
        console.log(`ProcessPayment invoice=${invoice.id} status=${invoice.status} billing_reason=${invoice.billing_reason} amount_paid=${invoice.amount_paid} customer=${invoice.customer}`);

        const repositoryResult: Result<SubscriptionEntity, SubscriptionErrors> = await this.GetCustomerIDFromInvoice(invoice)
            .asyncMap(customerID => this.Repository.GetSubscriptionForCustomer(customerID))
            .andThen(result => result);

        if (repositoryResult.isErr()) {
            return err(repositoryResult.error)
        }

        const subscriptionData: SubscriptionEntity = repositoryResult.value;
        console.log(`Loaded subscription from repository SubscriptionID=${subscriptionData.SubscriptionID} Status=${subscriptionData.Status} SubscriptionStatus=${subscriptionData.SubscriptionStatus} ExternalSubscriptionID=${subscriptionData.ExternalSubscriptionID}`);

        if (invoice.lines?.data.length > 0) {
            const priceID = this.GetPriceIDFromLine(invoice.lines.data[0]);
            const subscriptionID = invoice.lines.data[0].subscription as string;
            if (priceID.isErr()) {
                return err(priceID.error)
            }
            const price: string = priceID.value;
            if (price === subscriptionData.ExternalSubscriptionItemID) {
                if (invoice.status === 'paid') {
                    
                    // TODO: Cuando haya diferentes planes, descuentso etc, cambiar esto porque peta
                    if (invoice.billing_reason === 'subscription_create' && invoice.amount_paid == 0) {
                        console.log(`Invoice ${invoice.id} is paid for subscription ${invoice.lines.data[0].parent?.subscription_item_details?.subscription} but the amount is 0, setting status to Trialing...`);
                        const subscription : Stripe.Subscription = await this.StripeClient.subscriptions.retrieve(invoice.lines.data[0].parent?.subscription_item_details?.subscription!);
                        subscriptionData.ExternalSubscriptionID = subscription.id;
                        subscriptionData.SubscriptionStatus = 'Trialing';
                        subscriptionData.Status = 'OrderDelayed'
                        subscriptionData.StartDate = new Date(subscription.trial_end! * 1000).toISOString();
                        console.log(`Transitioned subscription to trial SubscriptionID=${subscriptionData.SubscriptionID} Status=${subscriptionData.Status} SubscriptionStatus=${subscriptionData.SubscriptionStatus} ExternalSubscriptionID=${subscriptionData.ExternalSubscriptionID}`);
                    }
                    else {
                        console.log(`Invoice ${invoice.id} is paid for subscription ${subscriptionData.ExternalSubscriptionItemID} renewing for one month`);
                        let endDate: Date = new Date(subscriptionData.StartDate);
                        endDate.setMonth(endDate.getMonth() + 1);
                        subscriptionData.EndDate = endDate.toISOString();
                        subscriptionData.SubscriptionStatus = 'Active';
                        subscriptionData.Status = 'InvoicePayed';
                        console.log(`Transitioned subscription to active SubscriptionID=${subscriptionData.SubscriptionID} Status=${subscriptionData.Status} SubscriptionStatus=${subscriptionData.SubscriptionStatus} ExternalSubscriptionID=${subscriptionData.ExternalSubscriptionID}`);
                    }
                    
                    return (await this.Repository.SaveSubscription(subscriptionData)).map(_ => void (0));
                } else {
                    subscriptionData.SubscriptionStatus = 'Inactive';
                    subscriptionData.ExternalSubscriptionID = subscriptionID;
                    subscriptionData.Status = 'PaymentFailed';
                    console.log(`Invoice ${invoice.id} was not paid for subscription ${subscriptionData.ExternalSubscriptionID} setting status to inactive`);
                    return (await this.Repository.SaveSubscription(subscriptionData)).map(_ => void (0));
                }
            } else {
                console.log(`Invoice ${invoice.id} received price ID ${price} and the subscription was for ${subscriptionData.ExternalSubscriptionID}`);
                return err(SubscriptionErrors.WRONG_PAYMENT_FOR_SUBSCRIPTION);
            }
        }

        return err(SubscriptionErrors.NO_LINES_IN_EVENT);
    }

    /**
     * Extracts the customer ID from a given Stripe invoice object.
     *
     * @param {Stripe.Invoice} invoice - The Stripe invoice object from which the customer ID is to be retrieved.
     * @return {Result<string, SubscriptionErrors>} A result containing the customer ID as a string if successful,
     *                                              or an error of type `SubscriptionErrors` if the customer ID is not found or is invalid.
     */
    private GetCustomerIDFromInvoice(invoice: Stripe.Invoice): Result<string, SubscriptionErrors> {
        if (typeof invoice.customer !== 'string' || !invoice.customer) {
            console.log(`The stripe customer is invalid or not expanded: ${invoice.customer}`);
            return err(SubscriptionErrors.CUSTOMER_NOT_FOUND);
        }
        return ok(invoice.customer);
    }

    /**
     * Extracts and returns the price ID from the given invoice line data.
     *
     * @param lineData The Stripe invoice line item containing pricing details.
     * @return A `Result` object containing the price ID as a string if successful,
     *         or a `SubscriptionErrors` enum value if the price ID could not be found.
     */
    private GetPriceIDFromLine(lineData: Stripe.InvoiceLineItem): Result<string, SubscriptionErrors> {

        if (!lineData.pricing?.price_details?.price) {
            return err(SubscriptionErrors.PRICE_ID_NOT_FOUND);
        }
        return ok(lineData.pricing?.price_details?.price as string);

    }
}
