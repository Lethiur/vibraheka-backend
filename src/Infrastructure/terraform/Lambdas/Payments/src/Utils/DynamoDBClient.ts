import AWS from 'aws-sdk';
import {DynamoDB} from 'aws-sdk';


const documentClient = new AWS.DynamoDB.DocumentClient();


export default class DynamoDBClient<T extends DynamoDB.DocumentClient.PutItemInputAttributeMap> {

    private readonly TableName: string = ""

    constructor(tableName: string) {
        this.TableName = tableName
    }

    async putItem(item: T): Promise<void> {
        const params: DynamoDB.DocumentClient.PutItemInput = {
            TableName: this.TableName,
            Item: item
        }

        try {
            await documentClient.put(params).promise();
            console.log('Item added successfully!');
        } catch (error) {
            console.error('Error putting item:', error);
        }
    }

    async queryIndexWithoutFilter(indexName: string, condition : string, attributes : any) : Promise<T[]> {
        const params: DynamoDB.DocumentClient.QueryInput = {
            TableName: this.TableName,
            IndexName: indexName, // replace with the name of your GSI
            KeyConditionExpression: condition,
            ExpressionAttributeValues: attributes
        };

        try {
            const result = await documentClient.query(params).promise();
            return (result.Items  as T[]) || [];
        } catch (error) {
            console.error('Error querying items:', error);
            throw error;
        }
    }

    async queryIndex(indexName: string, condition : string, filter: string, attributes : any) : Promise<T[]>
    {
        const params: DynamoDB.DocumentClient.QueryInput = {
            TableName: this.TableName,
            IndexName: indexName, // replace with the name of your GSI
            KeyConditionExpression: condition,
            FilterExpression: filter,
            ExpressionAttributeValues: attributes
        };

        try {
            const result = await documentClient.query(params).promise();
            return (result.Items  as T[]) || [];
        } catch (error) {
            console.error('Error querying items:', error);
            throw error;
        }
    }

    async delete(condition : any) : Promise<number> {
        const deleteParams: DynamoDB.DocumentClient.DeleteItemInput = {
            TableName: this.TableName,
            Key: condition
        };

        console.log("Deleting", deleteParams)


        try {
            await documentClient.delete(deleteParams).promise()
            return 1
        }
        catch (e) {
            console.log("Problem while deleting an element -> ", e)
            return 0
        }
    }

    async getByPrimaryKey( primaryKeyName : string, primaryKeyValue : any) : Promise<T> {
        const params: DynamoDB.DocumentClient.QueryInput = {
            TableName: this.TableName,
            KeyConditionExpression: `${primaryKeyName} = :pkValue`,
            ExpressionAttributeValues: {
                ":pkValue" : primaryKeyValue
            }
        };

        try {
            const result = await documentClient.query(params).promise();
            return (result.Items  as T[])[0] || null;
        } catch (error) {
            console.error('Error querying items:', error);
            throw error;
        }
    }

}