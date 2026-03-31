import {GetObjectCommand, S3Client} from "@aws-sdk/client-s3";
import {errAsync, ResultAsync} from "neverthrow";
import {EmailTemplateStorageErrors} from "@Domain/Errors/EmailTemplateStorageErrors";

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
    public getFileContents(key: string, bucketName: string): ResultAsync<string, EmailTemplateStorageErrors> {
        return ResultAsync.fromPromise(
            this.s3Client.send(
                new GetObjectCommand({
                    Bucket: bucketName,
                    Key: key
                })
            ),
            _error => EmailTemplateStorageErrors.ERROR_FETCHING_TEMPLATE
        ).andThen(response => {
            if (!response.Body) {
                return errAsync(EmailTemplateStorageErrors.TEMPLATE_NOT_FOUND);
            }
            return ResultAsync.fromPromise(
                response.Body.transformToString("utf-8"),
                _error => EmailTemplateStorageErrors.ERROR_FETCHING_TEMPLATE
            );
        });
    }
}
