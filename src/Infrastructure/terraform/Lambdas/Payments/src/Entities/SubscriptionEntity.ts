export default interface SubscriptionEntity {
    SubscriptionID : string;
    UserID: string;
    StartDate: Date;
    EndDate: Date;
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