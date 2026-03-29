import {Result, ResultAsync} from "neverthrow";
import EmailSenderErrors from "@Domain/Errors/EmailSenderErrors";

/**
 * Contract for resolving and rendering email templates.
 */
export default interface IEmailTemplateService {

    /**
     * Renders a template by replacing placeholders with corresponding values from the provided data.
     *
     * @param {string} templateParameterName - The name or identifier of the template to render.
     * @param {Record<string, string | number>} data - A key-value mapping of placeholders in the template to their replacement values.
     * @return {ResultAsync<string, EmailSenderErrors>} A ResultAsync object containing either the rendered template as a string or an EmailSenderErrors enumeration indicating an error.
     */
    RenderTemplate(templateParameterName: string, data: Record<string, string | number>): ResultAsync<string, EmailSenderErrors>;
    
}
