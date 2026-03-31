import {requireEnv} from "@/Validators/EnvironmentValidator";

/**
 * Represents a collection of predefined email templates used for various communication purposes.
 * These templates are initialized with environment-specific parameters.
 */
export default class EmailTemplates {
    
    public readonly VerificationTemplate: string;
    public readonly PasswordResetTemplate: string;
    public readonly UserWelcomeTemplate: string;
    public readonly SubscriptionThankYouTemplate: string;
    public readonly TrialEndingSoonTemplate: string;
    public readonly SubscriptionCancelledTemplate: string;
    public readonly SubscriptionReactivatedTemplate: string;
    
    
    constructor() {
        this.VerificationTemplate = requireEnv("SSM_VERIFICATION_TEMPLATE_NAME_PARAM");
        this.PasswordResetTemplate = requireEnv("SSM_PASSWORD_RESET_TEMPLATE_NAME_PARAM");
        this.UserWelcomeTemplate = requireEnv("SSM_USER_WELCOME_TEMPLATE_NAME_PARAM");
        this.SubscriptionThankYouTemplate = requireEnv("SSM_SUBSCRIPTION_THANK_YOU_TEMPLATE_NAME_PARAM");
        this.TrialEndingSoonTemplate = requireEnv("SSM_TRIAL_ENDING_SOON_TEMPLATE_NAME_PARAM");
        this.SubscriptionCancelledTemplate = requireEnv("SSM_SUBSCRIPTION_CANCELLED_TEMPLATE_NAME_PARAM");
        this.SubscriptionReactivatedTemplate = requireEnv("SSM_SUBSCRIPTION_REACTIVATED_TEMPLATE_NAME_PARAM");
    }
}

export const EmailTemplatesInstance = new EmailTemplates();
