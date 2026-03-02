import {GetObjectCommand, S3Client} from "@aws-sdk/client-s3";

/**
 * Thin wrapper around AWS S3 client used to fetch template content.
 */
export default class S3ClientWrapper {
    constructor(private readonly s3Client: S3Client = new S3Client()) {}

    /**
     * Loads a text file from S3 and returns its UTF-8 content.
     *
     * @param key Object key inside the bucket.
     * @param bucketName S3 bucket name.
     * @returns File content as string.
     * @throws Error when object does not exist or content is empty.
     */
    public async getFileContents(key: string, bucketName: string): Promise<string> {
        const response = await this.s3Client.send(
            new GetObjectCommand({
                Bucket: bucketName,
                Key: key
            })
        );

        if (!response.Body) {
            throw new Error(`File not found in S3: ${bucketName}/${key}`);
        }

        const templateHtml = await response.Body.transformToString("utf-8");
        if (!templateHtml) {
            throw new Error("File content is empty");
        }

        return templateHtml;
    }
}
