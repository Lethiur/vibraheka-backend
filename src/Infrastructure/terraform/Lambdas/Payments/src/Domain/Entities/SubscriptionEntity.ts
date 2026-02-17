export default interface SubscriptionEntity {
    SubscriptionID : string;
    UserID: string;
    StartDate: string;
    EndDate: string;
    ExternalSubscriptionID: string;
    ExternalSubscriptionItemID: string;
    ExternalCustomerID: string;
    OrderType: string;
    Status: string;
    SubscriptionStatus: string;
    Created: Date;
    CreatedBy: string;
    LastModified: Date;
    LastModifiedBy: string;
}