import SubscriptionEntity from "../../src/Domain/Entities/SubscriptionEntity";

export  const subEntry : SubscriptionEntity = {
    Created: new Date(),
    CreatedBy: "",
    EndDate: "",
    ExternalCustomerID: "",
    ExternalSubscriptionID: "",
    ExternalSubscriptionItemID: "",
    LastModified: new Date(),
    LastModifiedBy: "",
    OrderType: "",
    StartDate: "",
    Status: "",
    SubscriptionStatus: "",
    SubscriptionID: 'test-id',
    UserID: 'test-user'
};

export function serializeDates(obj: any): any {
    return JSON.parse(
        JSON.stringify(obj, (_, value) =>
            value instanceof Date ? value.toISOString() : value
        )
    );
}