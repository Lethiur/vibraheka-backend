import SubscriptionEntity from "@Domain/Entities/SubscriptionEntity";
import {Result} from "neverthrow";
import {SubscriptionErrors} from "@Domain/Errors/SubscriptionErrors";

/**
 * 
 */
export default interface ISubscriptionRepository {
    
    GetSubscriptionForCustomer(customerID: string): Promise<Result<SubscriptionEntity, SubscriptionErrors>>;
    
    SaveSubscription(subscription: SubscriptionEntity): Promise<Result<SubscriptionEntity, SubscriptionErrors>>;
    
    DeleteSubscription(subscriptionEntity : SubscriptionEntity) : Promise<Result<void, SubscriptionErrors>>;
}