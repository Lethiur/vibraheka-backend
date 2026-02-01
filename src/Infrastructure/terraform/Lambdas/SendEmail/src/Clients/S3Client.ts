import {S3Client, GetObjectCommand} from '@aws-sdk/client-s3';

export default class S3ClientWrapper {
    
    constructor(private readonly s3Client: S3Client = new S3Client()) {}
    
    public async getFileContents(key: string, bucketName: string): Promise<string> {
        const response = await this.s3Client.send(
            new GetObjectCommand({
                Bucket: bucketName,
                Key: key,
            })
        );

        if (!response.Body) {
            throw new Error(`File not found in S3: ${bucketName}/${key}`);
        }

        const templateHtml = await response.Body.transformToString('utf-8');

        if (!templateHtml) {
            throw new Error('File content is empty');
        }
        
        return templateHtml;

    }
}