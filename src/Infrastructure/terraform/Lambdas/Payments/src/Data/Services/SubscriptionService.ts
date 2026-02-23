import {SubscriptionErrors} from "@/Domain/Errors/SubscriptionErrors";
import ISubscriptionService from "@Domain/Interfaces/ISubscriptionService";
import {Result, err, ok} from "neverthrow";
import Stripe from "stripe";
import ISubscriptionRepository from "@Domain/Interfaces/ISubscriptionRepository";
import SubscriptionEntity from "@Domain/Entities/SubscriptionEntity";

/**
 * A service class responsible for handling subscription-related operations.
 * Implements the ISubscriptionService interface.
 */
export default class SubscriptionService implements ISubscriptionService {

    /**
     * Constructs an instance of the class with the provided subscription repository.
     *
     * @param {ISubscriptionRepository} Repository - The subscription repository used for data operations.
     */
    constructor(private readonly Repository: ISubscriptionRepository) {
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
            return (await this.Repository.SaveSubscription(subscriptionEntity)).map(_ => void (0));
        }

        return err(SubscriptionErrors.WRONG_PAYMENT_FOR_SUBSCRIPTION);
    }

    public async UpdateSubscription(subscriptionData: Stripe.Subscription): Promise<Result<void, SubscriptionErrors>> {

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
            console.log("Received event for update subscription with date", subscriptionData.cancel_at)
            subscriptionEntity.SubscriptionStatus = (subscriptionData.cancel_at ? 'ToBeCancelled' : 'Active');
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

        const repositoryResult: Result<SubscriptionEntity, SubscriptionErrors> = await this.GetCustomerIDFromInvoice(invoice)
            .asyncMap(customerID => this.Repository.GetSubscriptionForCustomer(customerID))
            .andThen(result => result);

        if (repositoryResult.isErr()) {
            return err(repositoryResult.error)
        }

        const subscriptionData: SubscriptionEntity = repositoryResult.value;

        if (invoice.lines.data.length > 0) {
            const priceID = this.GetPriceIDFromLine(invoice.lines.data[0]);
            const subscriptionID = invoice.lines.data[0].subscription as string;
            if (priceID.isErr()) {
                return err(priceID.error)
            }
            const price: string = priceID.value;
            if (price === subscriptionData.ExternalSubscriptionItemID) {
                if (invoice.status === 'paid') {
                    console.log(subscriptionData.StartDate)
                    let endDate: Date = new Date(subscriptionData.StartDate);
                    endDate.setMonth(endDate.getMonth() + 1);
                    subscriptionData.EndDate = endDate.toISOString();
                    subscriptionData.SubscriptionStatus = 'Active';
                    subscriptionData.ExternalSubscriptionID = subscriptionID;
                    subscriptionData.Status = 'InvoicePayed';
                    console.log(`Invoice ${invoice.id} is paid for subscription ${subscriptionData.ExternalSubscriptionItemID} renewing for one month`);
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

        try {
            return ok(lineData.pricing?.price_details?.price as string);
        } catch (error) {
            return err(SubscriptionErrors.PRICE_ID_NOT_FOUND);
        }

    }
}