import SubscriptionService from "../../../../src/Data/Services/SubscriptionService";
import ISubscriptionRepository from "../../../../src/Domain/Interfaces/ISubscriptionRepository";
import { mock, MockProxy } from 'jest-mock-extended';
import {err, ok, Result} from "neverthrow";
import SubscriptionEntity from "../../../../src/Domain/Entities/SubscriptionEntity";
import {SubscriptionErrors} from "../../../../src/Domain/Errors/SubscriptionErrors";
import {subEntry} from "../../../Utils/TestSubscriptionData";

describe('@SubscriptionService @CancelSubscription Test Suite', () => {

    let SubService: SubscriptionService;
    let mockRepository : MockProxy<ISubscriptionRepository>;
    beforeEach(() => {
        
        mockRepository = mock<ISubscriptionRepository>();
        SubService = new SubscriptionService(mockRepository);
    });

    it('should not allow to cancel a subscription that is not the one in DB', async () => {
        
        // Given: Some mocking
        mockRepository.GetSubscriptionForCustomer.calledWith(expect.anything()).mockResolvedValue(ok({
            ExternalSubscriptionID : "test"
        } as SubscriptionEntity));
        
        // When: Service is invoked
        const result : Result<void, SubscriptionErrors> = await SubService.CancelSubscription({customer: "1234567890", id: "1234567890"} as any);
        
        // Then: The result should be an error
        expect(result.isErr()).toBeTruthy();
        
        if (result.isErr()) {
            expect(result.error).toEqual(SubscriptionErrors.WRONG_PAYMENT_FOR_SUBSCRIPTION);
        }
        
        // And: repository should have been invoked only one time for reading
        expect(mockRepository.GetSubscriptionForCustomer).toHaveBeenCalledTimes(1);
    
        // And: repository for writing should not be invoked
        expect(mockRepository.SaveSubscription).not.toHaveBeenCalled();
    });

    it('should save the subscription just fine after the checks and shit', async () => {
        // Given: Some mocking
        mockRepository.GetSubscriptionForCustomer.calledWith(expect.anything()).mockResolvedValue(ok({
            ExternalSubscriptionID : "1234567890"
        } as SubscriptionEntity));
        
        mockRepository.SaveSubscription.calledWith(expect.anything()).mockResolvedValue(ok(subEntry));

        // When: Service is invoked
        const result : Result<void, SubscriptionErrors> = await SubService.CancelSubscription({customer: "1234567890", id: "1234567890"} as any);

        // Then: The result should be an error
        expect(result.isErr()).toBeFalsy();

        if (result.isOk()) {
            expect(result.value).toEqual(void(0));
        }

        // And: repository should have been invoked only one time for reading
        expect(mockRepository.GetSubscriptionForCustomer).toHaveBeenCalledTimes(1);

        // And: repository for writing should not be invoked
        expect(mockRepository.SaveSubscription).toHaveBeenCalledTimes(1);
        
        // And: The repository save call should have been called with the right data
        expect(mockRepository.SaveSubscription).toHaveBeenCalledWith(expect.objectContaining({
            ExternalSubscriptionID : "1234567890",
            SubscriptionStatus: 'Cancelled'
        }));
    });

    it('should forward the error from the repository', async () => {
        // Given: Some mocking
        mockRepository.GetSubscriptionForCustomer.calledWith(expect.anything()).mockResolvedValue(err(SubscriptionErrors.PRICE_ID_NOT_FOUND));

        mockRepository.SaveSubscription.calledWith(expect.anything()).mockResolvedValue(ok(subEntry));

        // When: Service is invoked
        const result : Result<void, SubscriptionErrors> = await SubService.CancelSubscription({customer: "1234567890", id: "1234567890"} as any);

        // Then: The result should be an error
        expect(result.isErr()).toBeTruthy();

        if (result.isErr()) {
            expect(result.error).toEqual(SubscriptionErrors.PRICE_ID_NOT_FOUND);
        }

        // And: repository should have been invoked with the customer ID
        expect(mockRepository.GetSubscriptionForCustomer).toHaveBeenCalledWith("1234567890");

        // And: No more calls to GetSubscriptionForCustomer
        expect(mockRepository.GetSubscriptionForCustomer).toHaveBeenCalledTimes(1);
        
        // And: repository for writing should not be invoked
        expect(mockRepository.SaveSubscription).not.toHaveBeenCalled();
    });
});