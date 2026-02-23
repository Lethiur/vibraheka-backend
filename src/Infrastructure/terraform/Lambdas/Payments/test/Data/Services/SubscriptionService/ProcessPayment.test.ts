import SubscriptionService from "../../../../src/Data/Services/SubscriptionService";
import ISubscriptionRepository from "../../../../src/Domain/Interfaces/ISubscriptionRepository";
import {MockProxy, mock} from 'jest-mock-extended';
import {ok, err, Result, Err} from "neverthrow";
import {subEntry} from "../../../Utils/TestSubscriptionData";
import {SubscriptionErrors} from "../../../../src/Domain/Errors/SubscriptionErrors";
import Stripe from "stripe";

describe('@SubscriptionService @ProcessPayment Test Suite', () => {

    let SubService: SubscriptionService;
    let mockRepository: MockProxy<ISubscriptionRepository>;
    beforeEach(() => {

        mockRepository = mock<ISubscriptionRepository>();
        SubService = new SubscriptionService(mockRepository);
    });

    it('should process the payment just fine', async () => {

        // Given: Some mocking
        const entity : any = {
            ExternalSubscriptionID: "test123-123",
            ExternalSubscriptionItemID: "test-123",
            StartDate: new Date()
        };
        
        mockRepository.GetSubscriptionForCustomer.calledWith(expect.anything()).mockResolvedValue(ok(entity));
        mockRepository.SaveSubscription.calledWith(expect.anything()).mockResolvedValue(ok(subEntry));

        // When: Services is invoked
        const serviceResult: Result<void, SubscriptionErrors> = await SubService.ProcessPayment({
            status: 'paid',
            id: "Holy shit bro",
            customer: "123456789",
            lines: {
                data: [
                    {
                        subscription: "sub_test-123",
                        pricing: {price_details: {price: "test-123"}}
                    } as Stripe.InvoiceLineItem] as Stripe.InvoiceLineItem[]
                
            }
        } as any);
        
        // Then: The result should be a success
        expect(serviceResult.isOk()).toBeTruthy()
        
        // And: The save method should have been called with the right parameters
        const endDate : Date = new Date(entity.StartDate);
        endDate.setMonth(endDate.getMonth() + 1);
        expect(mockRepository.SaveSubscription).toHaveBeenCalledWith(expect.objectContaining({
            SubscriptionStatus: 'Active',
            ExternalSubscriptionID: "sub_test-123",
            ExternalSubscriptionItemID: 'test-123',
            Status: 'InvoicePayed',
            StartDate: entity.StartDate,
            EndDate: endDate.toISOString()
        }));
    });

    it('should return an error if the subscription for the customer is not found', async () => {

        // Given: Some mocking
        mockRepository.GetSubscriptionForCustomer.calledWith(expect.anything()).mockResolvedValue(err(SubscriptionErrors.CUSTOMER_NOT_FOUND));
        
        mockRepository.SaveSubscription.calledWith(expect.anything()).mockResolvedValue(ok(subEntry));

        // When: Services is invoked
        const serviceResult: Result<void, SubscriptionErrors> = await SubService.ProcessPayment({
            id: "Holy shit bro",
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
        const error : Err<void, SubscriptionErrors> = serviceResult as Err<void, SubscriptionErrors>;
        expect(error.error).toBe(SubscriptionErrors.CUSTOMER_NOT_FOUND);
    });

    it('should return an error if the subscription for the customer is not a string', async () => {

        // Given: Some mocking
        mockRepository.GetSubscriptionForCustomer.calledWith(expect.anything()).mockResolvedValue(err(SubscriptionErrors.CUSTOMER_NOT_FOUND));

        mockRepository.SaveSubscription.calledWith(expect.anything()).mockResolvedValue(ok(subEntry));

        // When: Services is invoked
        const serviceResult: Result<void, SubscriptionErrors> = await SubService.ProcessPayment({
            id: "Holy shit bro",
            customer: 23,
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
        const error : Err<void, SubscriptionErrors> = serviceResult as Err<void, SubscriptionErrors>;
        expect(error.error).toBe(SubscriptionErrors.CUSTOMER_NOT_FOUND);
        
        // And: Save call should not have been executed
        expect(mockRepository.SaveSubscription).not.toHaveBeenCalled();
        
        // And: Call to retrieve the subs details should not be invoked
        expect(mockRepository.GetSubscriptionForCustomer).not.toHaveBeenCalled()
    });

    it('should handle the case where there is no price ID', async () => {
        // Given: Some mocking
        const entity : any = {
            ExternalSubscriptionID: "test123-123",
            ExternalSubscriptionItemID: "sub_test-123",
            StartDate: new Date()
        };

        mockRepository.GetSubscriptionForCustomer.calledWith(expect.anything()).mockResolvedValue(ok(entity));
        mockRepository.SaveSubscription.calledWith(expect.anything()).mockResolvedValue(ok(subEntry));

        // When: Services is invoked
        const serviceResult: Result<void, SubscriptionErrors> = await SubService.ProcessPayment({
            status: 'paid',
            id: "Holy shit bro",
            customer: "123456789",
            lines: {
                data: [
                    {
                        subscription: "sub_test-123",
                    } as Stripe.InvoiceLineItem] as Stripe.InvoiceLineItem[]

            }
        } as any);

        // Then: The result should be a success
        expect(serviceResult.isOk()).toBeFalsy();

        // And: The error should be CUSTOMER_NOT_FOUND
        const error : Err<void, SubscriptionErrors> = serviceResult as Err<void, SubscriptionErrors>;
        expect(error.error).toBe(SubscriptionErrors.PRICE_ID_NOT_FOUND);

        // And: Save call should not have been executed
        expect(mockRepository.SaveSubscription).not.toHaveBeenCalled();

        // And: Call to retrieve the subs details should not be invoked
        expect(mockRepository.GetSubscriptionForCustomer).toHaveBeenCalledWith('123456789')
    });

    it('should handle the case where no lines are received', async () => {

        // Given: Some mocking
        const entity : any = {
            ExternalSubscriptionID: "test123-123",
            ExternalSubscriptionItemID: "sub_test-123",
            StartDate: new Date()
        };

        mockRepository.GetSubscriptionForCustomer.calledWith(expect.anything()).mockResolvedValue(ok(entity));
        mockRepository.SaveSubscription.calledWith(expect.anything()).mockResolvedValue(ok(subEntry));

        // When: Services is invoked
        const serviceResult: Result<void, SubscriptionErrors> = await SubService.ProcessPayment({
            status: 'paid',
            id: "Holy shit bro",
            customer: "123456789"
        } as any);

        // Then: The result should be a success
        expect(serviceResult.isOk()).toBeFalsy();

        // And: The error should be CUSTOMER_NOT_FOUND
        const error : Err<void, SubscriptionErrors> = serviceResult as Err<void, SubscriptionErrors>;
        expect(error.error).toBe(SubscriptionErrors.NO_LINES_IN_EVENT);

        // And: Save call should not have been executed
        expect(mockRepository.SaveSubscription).not.toHaveBeenCalled();

        // And: Call to retrieve the subs details should not be invoked
        expect(mockRepository.GetSubscriptionForCustomer).toHaveBeenCalledWith('123456789')
    });

    it('should handle the case where the wrong subscription is paid for the one the customer has', async () => {
        // Given: Some mocking
        const entity : any = {
            ExternalSubscriptionID: "test123-123",
            ExternalSubscriptionItemID: "sub_test-123",
            StartDate: new Date()
        };

        mockRepository.GetSubscriptionForCustomer.calledWith(expect.anything()).mockResolvedValue(ok(entity));
        mockRepository.SaveSubscription.calledWith(expect.anything()).mockResolvedValue(ok(subEntry));

        // When: Services is invoked
        const serviceResult: Result<void, SubscriptionErrors> = await SubService.ProcessPayment({
            status: 'paid',
            id: "Holy shit bro",
            customer: "123456789",
            lines: {
                data: [
                    {
                        subscription: "sub_test-1234",
                        pricing: {price_details: {price: "test-123"}}
                    } as Stripe.InvoiceLineItem] as Stripe.InvoiceLineItem[]

            }
        } as any);

        // Then: The result should be a success
        expect(serviceResult.isOk()).toBeFalsy();

        // And: The error should be CUSTOMER_NOT_FOUND
        const error : Err<void, SubscriptionErrors> = serviceResult as Err<void, SubscriptionErrors>;
        expect(error.error).toBe(SubscriptionErrors.WRONG_PAYMENT_FOR_SUBSCRIPTION);

        // And: Save call should not have been executed
        expect(mockRepository.SaveSubscription).not.toHaveBeenCalled();

        // And: Call to retrieve the subs details should not be invoked
        expect(mockRepository.GetSubscriptionForCustomer).toHaveBeenCalledWith('123456789')
    });

    it('should handle the case when the invoce was not paid', async () => {
        // Given: Some mocking
        const entity : any = {
            ExternalSubscriptionID: "test123-123",
            ExternalSubscriptionItemID: "sub_test-123",
            StartDate: new Date()
        };

        mockRepository.GetSubscriptionForCustomer.calledWith(expect.anything()).mockResolvedValue(ok(entity));
        mockRepository.SaveSubscription.calledWith(expect.anything()).mockResolvedValue(ok(subEntry));

        // When: Services is invoked
        const serviceResult: Result<void, SubscriptionErrors> = await SubService.ProcessPayment({
            status: 'payment_failed',
            id: "Holy shit bro",
            customer: "123456789",
            lines: {
                data: [
                    {
                        subscription: "test123-123",
                        pricing: {price_details: {price: "sub_test-123"}}
                    } as Stripe.InvoiceLineItem] as Stripe.InvoiceLineItem[]

            }
        } as any);

        // Then: The result should be a success
        expect(serviceResult.isOk()).toBeTruthy();

        // And: Save call should not have been executed
        expect(mockRepository.SaveSubscription).toHaveBeenCalledWith(expect.objectContaining({
            Status: 'PaymentFailed',
            SubscriptionStatus: 'Inactive',
            ExternalSubscriptionID: 'test123-123'
        }));

        // And: Call to retrieve the subs details should not be invoked
        expect(mockRepository.GetSubscriptionForCustomer).toHaveBeenCalledWith('123456789')
    });
    
});