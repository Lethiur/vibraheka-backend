import SubscriptionService from "../../../../src/Data/Services/SubscriptionService";
import ISubscriptionRepository from "../../../../src/Domain/Interfaces/ISubscriptionRepository";
import {MockProxy, mock} from 'jest-mock-extended';
import {ok, err, Result, Err} from "neverthrow";
import {subEntry} from "../../../Utils/TestSubscriptionData";
import {SubscriptionErrors} from "../../../../src/Domain/Errors/SubscriptionErrors";
import Stripe from "stripe";

describe('@SubscriptionService @UpdateSubscription Test Suite', () => {

    let SubService: SubscriptionService;
    let mockRepository: MockProxy<ISubscriptionRepository>;
    beforeEach(() => {

        mockRepository = mock<ISubscriptionRepository>();
        SubService = new SubscriptionService(mockRepository);
    });


    it('should return an error if the subscription for the customer is not found', async () => {

        // Given: Some mocking
        mockRepository.GetSubscriptionForCustomer.calledWith(expect.anything()).mockResolvedValue(err(SubscriptionErrors.CUSTOMER_NOT_FOUND));

        mockRepository.SaveSubscription.calledWith(expect.anything()).mockResolvedValue(ok(subEntry));

        // When: Services is invoked
        const serviceResult: Result<void, SubscriptionErrors> = await SubService.UpdateSubscription({
            id: "Holy shit bro",
            customer: "cus_1234",
            lines: {
                data: [
                    {
                        subscription: "test-123",
                        pricing: {price_details: {price: "test-123"}}
                    } as Stripe.InvoiceLineItem] as Stripe.InvoiceLineItem[]

            }
        } as any);

        // Then: The result should be a success
        expect(serviceResult.isOk()).toBeFalsy();

        // And: The error should be CUSTOMER_NOT_FOUND
        const error: Err<void, SubscriptionErrors> = serviceResult as Err<void, SubscriptionErrors>;
        expect(error.error).toBe(SubscriptionErrors.CUSTOMER_NOT_FOUND);
    });

    it('should return an error if the subscription for the customer is not a string', async () => {

        // Given: Some mocking
        mockRepository.GetSubscriptionForCustomer.calledWith(expect.anything()).mockResolvedValue(err(SubscriptionErrors.CUSTOMER_NOT_FOUND));

        mockRepository.SaveSubscription.calledWith(expect.anything()).mockResolvedValue(ok(subEntry));

        // When: Services is invoked
        const serviceResult: Result<void, SubscriptionErrors> = await SubService.UpdateSubscription({
            id: "Holy shit bro",
            customer: 23
        } as any);

        // Then: The result should be a success
        expect(serviceResult.isOk()).toBeFalsy();

        // And: The error should be CUSTOMER_NOT_FOUND
        const error: Err<void, SubscriptionErrors> = serviceResult as Err<void, SubscriptionErrors>;
        expect(error.error).toBe(SubscriptionErrors.CUSTOMER_NOT_FOUND);

        // And: Save call should not have been executed
        expect(mockRepository.SaveSubscription).not.toHaveBeenCalled();

        // And: Call to retrieve the subs details should not be invoked
        expect(mockRepository.GetSubscriptionForCustomer).toHaveBeenCalledWith(23)
    });

    it('should handle the error of subscription not found in the repository', async () => {
        // Given: Some mocking
        mockRepository.GetSubscriptionForCustomer.calledWith(expect.anything()).mockResolvedValue(err(SubscriptionErrors.SUBSCRIPTION_NOT_FOUND));

        // When: Services is invoked
        const serviceResult: Result<void, SubscriptionErrors> = await SubService.UpdateSubscription({
            id: "Holy shit bro",
            customer: 23
        } as any);

        // Then: The result should be a success
        expect(serviceResult.isOk()).toBeFalsy();

        // And: The error should be CUSTOMER_NOT_FOUND
        const error: Err<void, SubscriptionErrors> = serviceResult as Err<void, SubscriptionErrors>;
        expect(error.error).toBe(SubscriptionErrors.SUBSCRIPTION_NOT_FOUND);

        // And: Save call should not have been executed
        expect(mockRepository.SaveSubscription).not.toHaveBeenCalled();

        // And: Call to retrieve the subs details should not be invoked
        expect(mockRepository.GetSubscriptionForCustomer).toHaveBeenCalledWith(23)
    });

    it('should handle the case where the update is for a different subscription than the customers', async () => {

        // Given: Some mocking
        const entity: any = {
            ExternalSubscriptionID: "test123-123",
            ExternalSubscriptionItemID: "sub_test-123",
            StartDate: new Date()
        };

        mockRepository.GetSubscriptionForCustomer.calledWith(expect.anything()).mockResolvedValue(ok(entity));
        mockRepository.SaveSubscription.calledWith(expect.anything()).mockResolvedValue(ok(subEntry));

        // When: Services is invoked
        const serviceResult: Result<void, SubscriptionErrors> = await SubService.UpdateSubscription({
            status: 'paid',
            id: "Holy shit bro",
            customer: "123456789"
        } as any);

        // Then: The result should be a success
        expect(serviceResult.isOk()).toBeFalsy();

        // And: The error should be CUSTOMER_NOT_FOUND
        const error: Err<void, SubscriptionErrors> = serviceResult as Err<void, SubscriptionErrors>;
        expect(error.error).toBe(SubscriptionErrors.WRONG_PAYMENT_FOR_SUBSCRIPTION);

        // And: Save call should not have been executed
        expect(mockRepository.SaveSubscription).not.toHaveBeenCalled();

        // And: Call to retrieve the subs details should not be invoked
        expect(mockRepository.GetSubscriptionForCustomer).toHaveBeenCalledWith('123456789')
    });

    it('should handle the case where the subscription is cancelled', async () => {
        // Given: Some mocking
        const entity: any = {
            ExternalSubscriptionID: "sub_test123",
            ExternalSubscriptionItemID: "sub_test123",
            StartDate: new Date()
        };

        mockRepository.GetSubscriptionForCustomer.calledWith(expect.anything()).mockResolvedValue(ok(entity));
        mockRepository.SaveSubscription.calledWith(expect.anything()).mockResolvedValue(ok(subEntry));

        // When: Services is invoked
        const cancelDate = new Date();
        const serviceResult: Result<void, SubscriptionErrors> = await SubService.UpdateSubscription({
            id: "sub_test123",
            customer: "123456789",
            cancel_at: cancelDate.toISOString()
        } as any);

        console.log(serviceResult);

        // Then: The result should be a success
        expect(serviceResult.isOk()).toBeTruthy()

        // And: The save method should have been called with the right parameters
        expect(mockRepository.SaveSubscription).toHaveBeenCalledWith(expect.objectContaining({
            SubscriptionStatus: 'ToBeCancelled'
        }));
    });

    it('should handle the case where the subscription is reactivated', async () => {
        // Given: Some mocking
        const entity: any = {
            ExternalSubscriptionID: "sub_test123",
            ExternalSubscriptionItemID: "sub_test123",
            StartDate: new Date()
        };

        mockRepository.GetSubscriptionForCustomer.calledWith(expect.anything()).mockResolvedValue(ok(entity));
        mockRepository.SaveSubscription.calledWith(expect.anything()).mockResolvedValue(ok(subEntry));

        // When: Services is invoked
        const serviceResult: Result<void, SubscriptionErrors> = await SubService.UpdateSubscription({
            id: "sub_test123",
            customer: "123456789"
        } as any);

        console.log(serviceResult);

        // Then: The result should be a success
        expect(serviceResult.isOk()).toBeTruthy()

        // And: The save method should have been called with the right parameters
        expect(mockRepository.SaveSubscription).toHaveBeenCalledWith(expect.objectContaining({
            SubscriptionStatus: 'Active'
        }));
    });
});