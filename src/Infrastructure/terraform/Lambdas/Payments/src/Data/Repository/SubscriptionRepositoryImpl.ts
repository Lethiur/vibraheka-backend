import GenericDynamoDBRepository from "@Data/Repository/GenericDynamoDBRepository";
import SubscriptionEntity from "@Domain/Entities/SubscriptionEntity";
import ISubscriptionRepository from "@Domain/Interfaces/ISubscriptionRepository";
import {Result} from "neverthrow";
import {SubscriptionErrors} from "@Domain/Errors/SubscriptionErrors";
import {DynamoDBErrors} from "@Data/Errors/DynamoDBErrors";
import {DynamoDBClient} from "@aws-sdk/client-dynamodb";

/**
 * Implementation of the ISubscriptionRepository interface, providing methods for managing
 * subscription entities in a DynamoDB table.
 *
 * This class extends the GenericDynamoDBRepository, which supplies basic CRUD operations
 * for interacting with DynamoDB. Additional repository-specific logic is implemented here.
 *
 * Key functionalities include saving a subscription and retrieving a subscription by a customer's ID.
 * Errors specific to DynamoDB operations are also handled and mapped to subscription-specific errors.
 *
 * @extends {GenericDynamoDBRepository<SubscriptionEntity>}
 * @implements {ISubscriptionRepository}
 */
export default class SubscriptionRepositoryImpl extends GenericDynamoDBRepository<SubscriptionEntity> implements ISubscriptionRepository {

    constructor(Client: DynamoDBClient) {
        console.log(process.env.DYNAMO_TABLE_NAME);
        super(process.env.DYNAMO_TABLE_NAME!, Client);
    }

    /**
     * Saves a subscription entity to the database.
     *
     * @param {SubscriptionEntity} subscription - The subscription entity to be saved.
     * @return {Promise<Result<SubscriptionEntity, SubscriptionErrors>>} A promise that resolves to a result containing either the saved subscription entity or a set of subscription-related errors.
     */
    public async SaveSubscription(subscription: SubscriptionEntity): Promise<Result<SubscriptionEntity, SubscriptionErrors>> {
        const dynamoDBResult: Result<SubscriptionEntity, DynamoDBErrors> = await this.PutItem(subscription);
        return dynamoDBResult.mapErr(this.HandleDynamoError)
    }

    /**
     * Retrieves the subscription associated with a specific customer.
     * Queries the database using the provided customer ID and returns the first matching subscription.
     *
     * @param {string} customerID - The unique identifier of the customer whose subscription is being retrieved.
     * @return {Promise<Result<SubscriptionEntity, SubscriptionErrors>>} A promise resolving to a result object containing either the subscription entity on success or subscription-related errors on failure.
     */
    public async GetSubscriptionForCustomer(customerID: string): Promise<Result<SubscriptionEntity, SubscriptionErrors>> {
        console.log(`Getting subscription for customer ${customerID} from table ${process.env.DYNAMO_TABLE_NAME}`);
        const dynamoDBResult: Result<SubscriptionEntity[], DynamoDBErrors> = await this.QueryIndexWithoutFilter('ExternalCustomer-Index', `ExternalCustomerID = :customerID`, {":customerID": {"S" : customerID}});
        return dynamoDBResult.map(subList => subList[0]).mapErr(this.HandleDynamoError)
    }

    public async DeleteSubscription(subscriptionEntity: SubscriptionEntity): Promise<Result<void, SubscriptionErrors>> {
        const deleteResult: Result<number, DynamoDBErrors> = await this.Delete({
            SubscriptionID: { S: subscriptionEntity.SubscriptionID }
        });

        return deleteResult.map(_ => void (0)).mapErr(this.HandleDynamoError);
    }

    /**
     * Handles errors received from DynamoDB and maps them to subscription-specific errors.
     *
     * @param dynamoError - The error received from DynamoDB.
     * @return The corresponding subscription error based on the provided DynamoDB error.
     */
    private HandleDynamoError(dynamoError: DynamoDBErrors): SubscriptionErrors {

        switch (dynamoError) {
            case DynamoDBErrors.ITEM_NOT_FOUND:
                return SubscriptionErrors.SUBSCRIPTION_NOT_FOUND;
            default:
                console.error(`Unexpected error occurred while interacting with DynamoDB: ${dynamoError}`);
                return SubscriptionErrors.UNEXPECTED_ERROR;
        }
    }

}
