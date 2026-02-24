import SubscriptionRepositoryImpl from "../../../../src/Data/Repository/SubscriptionRepositoryImpl";
import {AttributeValue, DynamoDBClient, QueryCommand} from "@aws-sdk/client-dynamodb";
import {mockClient} from 'aws-sdk-client-mock';
import SubscriptionEntity from "../../../../src/Domain/Entities/SubscriptionEntity";
import {marshall} from "@aws-sdk/util-dynamodb";
import {serializeDates, subEntry} from "../../../Utils/TestSubscriptionData";
import {SubscriptionErrors} from "../../../../src/Domain/Errors/SubscriptionErrors";
import {Err, Ok, Result} from "neverthrow";

const dynamoMock = mockClient(DynamoDBClient)

describe('@SubscriptionRepository @GetSubscription Test Suite', () => {
    let SubsRepo : SubscriptionRepositoryImpl;

    beforeEach(() => {
        dynamoMock.reset();
        process.env.DYNAMO_TABLE_NAME = 'TestTable';
        SubsRepo = new SubscriptionRepositoryImpl(dynamoMock as any);
    })

    it('should retrieve the list properly without any errors', async () => {
        
        // Given: A list of entities 
        const entityList : Record<string, AttributeValue>[] = [marshall(serializeDates(subEntry))]
        
        // And: Mongo mocked
        dynamoMock.on(QueryCommand).resolves({$metadata: {httpStatusCode: 200, requestId: 'abc'}, Items: entityList});
        
        // When: Repository is invoked
        const repoResult : Result<SubscriptionEntity, SubscriptionErrors> = await SubsRepo.GetSubscriptionForCustomer('1234567890');
        
        // Then: Result should be ok
        expect(repoResult.isOk()).toBeTruthy();
                
        // And: Result should be the expected one
        const okResult : Ok<SubscriptionEntity, SubscriptionErrors> = repoResult as Ok<SubscriptionEntity, SubscriptionErrors>;
        expect(okResult.value).toEqual(serializeDates(subEntry));
        
        // And: The mock call should have the required args
        const calls = dynamoMock.commandCalls(QueryCommand);

        expect(calls.length).toBe(1);

        const sentCommand = calls[0].firstArg;

        expect(sentCommand.input).toEqual({
            TableName: "TestTable",
            IndexName: "ExternalCustomer-Index",
            KeyConditionExpression: `ExternalCustomerID = :customerID`,
            ExpressionAttributeValues: {":customerID": {"S" : "1234567890"}}
        });
    });

    it('should propagate the when there is no item returned', async () => {
        // Given: A list of entities 
        const entityList : Record<string, AttributeValue>[] = [marshall(serializeDates(subEntry))]

        // And: Mongo mocked
        dynamoMock.on(QueryCommand).resolves({$metadata: {httpStatusCode: 200, requestId: 'abc'}, Items: []});

        // When: Repository is invoked
        const repoResult : Result<SubscriptionEntity, SubscriptionErrors> = await SubsRepo.GetSubscriptionForCustomer('1234567890');

        // Then: Result should be ok
        expect(repoResult.isOk()).toBeFalsy();

        // And: Result should be the expected one
        const failureResult : Err<SubscriptionEntity, SubscriptionErrors> = repoResult as Err<SubscriptionEntity, SubscriptionErrors>;
       
        expect(failureResult.error).toEqual(SubscriptionErrors.SUBSCRIPTION_NOT_FOUND);
        
        // And: The mock call should have the required args
        const calls = dynamoMock.commandCalls(QueryCommand);

        expect(calls.length).toBe(1);

        const sentCommand = calls[0].firstArg;

        expect(sentCommand.input).toEqual({
            TableName: "TestTable",
            IndexName: "ExternalCustomer-Index",
            KeyConditionExpression: `ExternalCustomerID = :customerID`,
            ExpressionAttributeValues: {":customerID": {"S" : "1234567890"}}
        });
    });

    it('should propagate the when the items list is undefined', async () => {
        // Given: A list of entities 
        const entityList : Record<string, AttributeValue>[] = [marshall(serializeDates(subEntry))]

        // And: Mongo mocked
        dynamoMock.on(QueryCommand).resolves({$metadata: {httpStatusCode: 200, requestId: 'abc'}, Items: undefined});

        // When: Repository is invoked
        const repoResult : Result<SubscriptionEntity, SubscriptionErrors> = await SubsRepo.GetSubscriptionForCustomer('1234567890');

        // Then: Result should be ok
        expect(repoResult.isOk()).toBeFalsy();

        // And: Result should be the expected one
        const failureResult : Err<SubscriptionEntity, SubscriptionErrors> = repoResult as Err<SubscriptionEntity, SubscriptionErrors>;

        expect(failureResult.error).toEqual(SubscriptionErrors.SUBSCRIPTION_NOT_FOUND);

        // And: The mock call should have the required args
        const calls = dynamoMock.commandCalls(QueryCommand);

        expect(calls.length).toBe(1);

        const sentCommand = calls[0].firstArg;

        expect(sentCommand.input).toEqual({
            TableName: "TestTable",
            IndexName: "ExternalCustomer-Index",
            KeyConditionExpression: `ExternalCustomerID = :customerID`,
            ExpressionAttributeValues: {":customerID": {"S" : "1234567890"}}
        });
    });

    it('should propagate the when the dynamo DB repository throws an error', async () => {
        // Given: A list of entities 
        const entityList : Record<string, AttributeValue>[] = [marshall(serializeDates(subEntry))]

        // And: Mongo mocked
        dynamoMock.on(QueryCommand).rejects(new Error("DynamoDB error"));

        // When: Repository is invoked
        const repoResult : Result<SubscriptionEntity, SubscriptionErrors> = await SubsRepo.GetSubscriptionForCustomer('1234567890');

        // Then: Result should be ok
        expect(repoResult.isOk()).toBeFalsy();

        // And: Result should be the expected one
        const failureResult : Err<SubscriptionEntity, SubscriptionErrors> = repoResult as Err<SubscriptionEntity, SubscriptionErrors>;

        expect(failureResult.error).toEqual(SubscriptionErrors.UNEXPECTED_ERROR);

        // And: The mock call should have the required args
        const calls = dynamoMock.commandCalls(QueryCommand);

        expect(calls.length).toBe(1);

        const sentCommand = calls[0].firstArg;

        expect(sentCommand.input).toEqual({
            TableName: "TestTable",
            IndexName: "ExternalCustomer-Index",
            KeyConditionExpression: `ExternalCustomerID = :customerID`,
            ExpressionAttributeValues: {":customerID": {"S" : "1234567890"}}
        });
    });
});