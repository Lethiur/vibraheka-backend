import {
    AttributeValue,
    DynamoDBClient,
    DynamoDBServiceException,
    PutItemCommand,
} from "@aws-sdk/client-dynamodb";
import { mockClient } from 'aws-sdk-client-mock';
import SubscriptionRepositoryImpl from "../../../../src/Data/Repository/SubscriptionRepositoryImpl";
import SubscriptionEntity from "../../../../src/Domain/Entities/SubscriptionEntity";
import {subEntry} from "../../../Utils/TestSubscriptionData";
import {SubscriptionErrors} from "../../../../src/Domain/Errors/SubscriptionErrors";
import {Result} from "neverthrow";
import {marshall} from "@aws-sdk/util-dynamodb";


const dynamoMock = mockClient(DynamoDBClient)

describe('@SubscriptionRepository @SaveSubscription test suite', () => {
    
    let SubsRepo : SubscriptionRepositoryImpl;
    
    beforeEach(() => {
        dynamoMock.reset();
        process.env.DYNAMO_TABLE_NAME = 'TestTable';
        SubsRepo = new SubscriptionRepositoryImpl(dynamoMock as any);
    })
    
    it('should save properly the given entity', async () => {
        // Given: An entity to save
        const subscriptionEntity : Record<string, AttributeValue> = marshall(subEntry, {convertClassInstanceToMap: true});
        
        // And: Some mocks for the dynamo client
        dynamoMock.on(PutItemCommand).resolves({
            $metadata: {
                httpStatusCode: 200,
                requestId: 'abc'
            }
        });
        
        // When: Repository is invoked
        const result : Result<SubscriptionEntity, SubscriptionErrors> = await SubsRepo.SaveSubscription(subscriptionEntity as any);
        
        // Then: Result should be ok 
        expect(result.isOk()).toBeTruthy();
        
        if (result.isOk()) {
            // And: The result should contain the given entity
            expect(result.value).toEqual(subscriptionEntity);    
        }
    });

    it('should propagate the error from the repository', async () => {
        
        // Given: An entity
        const subscriptionEntity : Record<string, AttributeValue> = marshall(subEntry, {convertClassInstanceToMap: true});
        
        // And: some mocking
        dynamoMock.on(PutItemCommand).rejects(new DynamoDBServiceException({
            name: 'ResourceNotFoundException',
            message: 'Table not found',
            $fault: 'client',
            $metadata: { httpStatusCode: 404 }
        }));
        
        // When: Repository is invoked
        const result : Result<SubscriptionEntity, SubscriptionErrors> = await SubsRepo.SaveSubscription(subscriptionEntity as any);
        
        // Then: The result should be an error
        expect(result.isErr()).toBeTruthy();
        
        if (result.isErr()) {
            expect(result.error).toEqual(SubscriptionErrors.UNEXPECTED_ERROR);
        }
    });
});