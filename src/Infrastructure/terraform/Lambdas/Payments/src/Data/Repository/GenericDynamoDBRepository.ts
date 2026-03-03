import {
    DynamoDBClient,
    DynamoDBServiceException,
    PutItemCommand,
    PutItemCommandOutput,
    QueryCommand,
    QueryCommandOutput,
    DeleteItemCommand,
    DeleteItemCommandOutput,
    AttributeValue
} from "@aws-sdk/client-dynamodb";
import {marshall, unmarshall} from "@aws-sdk/util-dynamodb";
import {DynamoDBErrors} from "@Data/Errors/DynamoDBErrors";
import {err, ok, Result} from "neverthrow";

/**
 * A generic repository class for handling operations on a DynamoDB table.
 * This class provides methods for CRUD operations and querying data.
 *
 * @template T The type of the items stored in the DynamoDB table.
 */
export default class GenericDynamoDBRepository<T> {

    /**
     * Creates an instance of the class with the specified table name and DynamoDB client.
     *
     * @param {string} TableName - The name of the DynamoDB table to interact with.
     * @param {DynamoDBClient} DynamoDBClient - An instance of the AWS DynamoDB client used for database operations.
     */
    constructor(private readonly TableName: string, private readonly DynamoDBClient: DynamoDBClient) {
    }

    /**
     * Stores a given item in the DynamoDB table.
     *
     * @param {T} item The item to be stored in the DynamoDB table.
     * @return {Promise<Result<T, DynamoDBErrors>>} A promise that resolves to a result object indicating success or failure of the operation.
     */
    public async PutItem(item: T): Promise<Result<T, DynamoDBErrors>> {
        const params: PutItemCommand = new PutItemCommand({
            TableName: this.TableName,
            Item: marshall(item, {removeUndefinedValues: true}),
        });

        try {
            const putOutput: PutItemCommandOutput = await this.DynamoDBClient.send(params);
            const httpStatusCode: number | undefined = putOutput.$metadata.httpStatusCode;
            if (httpStatusCode != 200) {
                console.error('Error putting item:', putOutput);
                return err(DynamoDBErrors.UNEXPECTED_ERROR);
            }
            console.log(`Successfully saved: ${JSON.stringify(item)} requestID: ${putOutput.$metadata.requestId}`);
            return ok(item);
        } catch (error) {
            console.log(error);
            return err(this.HandleError(error as Error))
        }
    }

    /**
     * Queries a specified index in a DynamoDB table without applying additional filters.
     *
     * @param {string} indexName - The name of the DynamoDB index to query.
     * @param {string} condition - The key condition expression to use for the query.
     * @param {Object} attributes - A map of attribute names to attribute values to substitute in the key condition expression.
     * @return {Promise<T[]>} A promise that resolves to an array of items retrieved from the query.
     */
    async QueryIndexWithoutFilter(indexName: string, condition: string, attributes: any): Promise<Result<T[], DynamoDBErrors>> {
        const params: QueryCommand = new QueryCommand({
            TableName: this.TableName,
            IndexName: indexName,
            KeyConditionExpression: condition,
            ExpressionAttributeValues: attributes
        });

        try {
            const result: QueryCommandOutput = await this.DynamoDBClient.send(params);
            if (result.$metadata.httpStatusCode != 200) {
                console.error('Error putting item:', result);
                return err(DynamoDBErrors.UNEXPECTED_ERROR);
            }
            console.log(`Query index without filter operation executed properly, request ID: ${result.$metadata.requestId}`)

            if (result.Items && result.Items.length > 0) {
                return ok(result.Items.map(item => unmarshall(item)) as T[]);
            } else {
                console.log(`No items found in the index. ${indexName}`);
                return err(DynamoDBErrors.ITEM_NOT_FOUND);
            }
        } catch (error) {
            console.log(error);
            return err(this.HandleError(error as Error))
        }
    }

    /**
     * Queries a DynamoDB table's Global Secondary Index (GSI) based on the specified condition and filter.
     *
     * @param {string} indexName - The name of the Global Secondary Index to query.
     * @param {string} condition - The Key Condition Expression used to identify the requested items in the index.
     * @param {string} filter - The Filter Expression to further refine the results returned by the query.
     * @param {any} attributes - The expression attribute values used in the key condition and filter expressions.
     * @return {Promise<Result<T[], DynamoDBErrors>>} A promise resolving to a `Result` object containing an array of the matching items or an error if the query fails.
     */
    async QueryIndex(indexName: string, condition: string, filter: string, attributes: any): Promise<Result<T[], DynamoDBErrors>> {

        const params: QueryCommand = new QueryCommand({
            TableName: this.TableName,
            IndexName: indexName, // replace with the name of your GSI
            KeyConditionExpression: condition,
            FilterExpression: filter,
            ExpressionAttributeValues: attributes
        });


        try {
            const result: QueryCommandOutput = await this.DynamoDBClient.send(params);
            if (result.$metadata.httpStatusCode != 200) {
                console.error('Error putting item:', result);
                return err(DynamoDBErrors.UNEXPECTED_ERROR);
            }
            console.log(`Query index operation executed properly, request ID: ${result.$metadata.requestId}`)

            if (result.Items && result.Items.length > 0) {
                return ok(result.Items as T[]);
            } else {
                return err(DynamoDBErrors.ITEM_NOT_FOUND);
            }
        } catch (error) {
            return err(this.HandleError(error as Error))
        }
    }

    /**
     * Deletes an item from the DynamoDB table based on the specified condition (key).
     *
     * @param {Record<string, AttributeValue>} condition - The key-value pair representing the primary key of the item to be deleted from the DynamoDB table.
     * @return {Promise<Result<number, DynamoDBErrors>>} A promise that resolves to a `Result` type indicating success with a value of 1 if the operation succeeds, or an error of type `DynamoDBErrors` if the operation fails.
     */
    public async Delete(condition:  Record<string, AttributeValue>): Promise<Result<number, DynamoDBErrors>> {
        const deleteParams: DeleteItemCommand = new DeleteItemCommand({
            TableName: this.TableName,
            Key: condition
        });

        try {
           const output : DeleteItemCommandOutput = await this.DynamoDBClient.send(deleteParams)
            
            if (output.$metadata.httpStatusCode != 200) {
                console.error('Error deleting item:', output);
                return err(DynamoDBErrors.UNEXPECTED_ERROR);
            }
            
            console.log(`Delete operation executed properly, request ID: ${output.$metadata.requestId}`)
            return ok(1);
            
            
        } catch (error) {
            return err(this.HandleError(error as Error))
        }
    }

    async GetByPrimaryKey(primaryKeyName: string, primaryKeyValue: any): Promise<Result<T, DynamoDBErrors>> {
        const params: QueryCommand = new QueryCommand({
            TableName: this.TableName,
            KeyConditionExpression: `${primaryKeyName} = :pkValue`,
            ExpressionAttributeValues: {
                ":pkValue": primaryKeyValue
            }
        });

        try {
            const result: QueryCommandOutput = await this.DynamoDBClient.send(params);
            if (result.$metadata.httpStatusCode != 200) {
                console.error('Error putting item:', result);
                return err(DynamoDBErrors.UNEXPECTED_ERROR);
            }
            console.log(`Query index operation executed properly, request ID: ${result.$metadata.requestId}`)
            if (result.Items && result.Items.length > 0) {
                return ok(result.Items[0] as T);
            } else {
                return err(DynamoDBErrors.ITEM_NOT_FOUND);
            }
        } catch (error) {
            return err(this.HandleError(error as Error))
        }
    }

    /**
     * Handles errors encountered during DynamoDB operations and categorizes them.
     *
     * @param {Error} error - The error object encountered during the operation.
     * @return {DynamoDBErrors} The categorized error type for the DynamoDB operation.
     */
    private HandleError(error: Error): DynamoDBErrors {
        console.error('Error executing DynamoDB operation:', error?.constructor?.name);
        if (
            typeof error === "object" &&
            error !== null &&
            "name" in error &&
            "$metadata" in error
        ) {
            return this.HandleDynamoDBError(error as DynamoDBServiceException);
        }
        return DynamoDBErrors.UNEXPECTED_ERROR;
    }

    /**
     * Handles errors returned from DynamoDB operations and maps them to predefined DynamoDB error types.
     *
     * @param error The exception object thrown by the DynamoDB service, containing the error details and name.
     * @return A specific `DynamoDBErrors` enum value corresponding to the type of error encountered.
     */
    private HandleDynamoDBError(error: DynamoDBServiceException): DynamoDBErrors {
        switch (error.name) {
            case "ConditionalCheckFailedException":
                return DynamoDBErrors.CONDITIONAL_CHECK_FAILED;
            case "ProvisionedThroughputExceededException":
                return DynamoDBErrors.PROVISIONED_THROUGHPUT_EXCEEDED;
            case "ResourceNotFoundException":
                return DynamoDBErrors.RESOURCE_NOT_FOUND;
            case "AccessDeniedException":
                return DynamoDBErrors.ACCESS_DENIED;
            default:
                return DynamoDBErrors.UNEXPECTED_ERROR;
        }
    }


}