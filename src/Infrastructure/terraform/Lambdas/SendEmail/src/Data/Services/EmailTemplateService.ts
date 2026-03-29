import S3ClientWrapper from "@/Clients/S3Client";
import SSMClientWrapper from "@/Clients/SSMClient";
import IEmailTemplateService from "@Domain/Interfaces/IEmailTemplateService";
import {err, ok, Result, ResultAsync} from "neverthrow";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";

/**
 * Resolves template IDs from SSM, loads HTML templates from S3 and renders placeholders.
 */
export default class EmailTemplateService implements IEmailTemplateService {
    constructor(
        private readonly ssmClient: SSMClientWrapper,
        private readonly s3Client: S3ClientWrapper,
        private readonly templateBucket: string,
    ) {
    }
    
    public RenderTemplate(templateParameterName: string, data: Record<string, string | number>): ResultAsync<string, EmailSenderErrors> {
        console.log("Rendering generic email template", {templateParameterName, keys: Object.keys(data)});
        return this.GetTemplateHtml(templateParameterName).andThen(templateHtml => this.ProcessTemplate(templateHtml, data));
    }

    /**
     * Retrieves template content from SSM + S3.
     *
     * @param templateParameterName SSM parameter with template identifier.
     * @returns Async result containing template HTML or domain error.
     */
    private GetTemplateHtml(templateParameterName: string): ResultAsync<string, EmailSenderErrors> {
        console.log("Resolving template name from SSM", {templateParameterName});
        return ResultAsync.fromPromise(this.ssmClient.getParameter(templateParameterName), (error) => {
            console.error("Failed resolving template from SSM", {templateParameterName, error});
            return EmailSenderErrors.TEMPLATE_RESOLUTION_FAILED;
        }).andThen(templateName => ResultAsync.fromPromise(
            this.s3Client.getFileContents(`${templateName}/template.json`, this.templateBucket),
            (error) => {
                console.error("Failed loading template from S3", {
                    templateName,
                    templateBucket: this.templateBucket,
                    error
                });
                return EmailSenderErrors.TEMPLATE_RESOLUTION_FAILED;
            }
        ));
    }

    /**
     * Replaces template placeholders using the provided key/value map.
     *
     * @param template Raw HTML template.
     * @param data Placeholder values.
     * @returns Result containing rendered HTML or domain error.
     */
    private ProcessTemplate(template: string, data: Record<string, string | number>): Result<string, EmailSenderErrors> {
        try {
            console.log("Processing template placeholders", {keys: Object.keys(data)});
            let processedHtml = template;

            for (const [key, value] of Object.entries(data)) {
                const placeholderRegex = new RegExp(`{{${key}}}`, "g");
                processedHtml = processedHtml.replace(placeholderRegex, String(value));
            }

            return ok(processedHtml);
        } catch (error) {
            console.error("Failed processing template placeholders", {error});
            return err(EmailSenderErrors.TEMPLATE_RENDER_FAILED);
        }
    }
}
