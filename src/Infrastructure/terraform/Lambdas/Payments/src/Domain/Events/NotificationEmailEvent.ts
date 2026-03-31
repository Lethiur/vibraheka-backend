export default interface NotificationEmailEventDetail {
    username: string;
    recipient: string;
    subject: string;
    templateType: "subscription_thank_you" | "trial_ending_soon" | "subscription_reactivated" | "subscription_cancelled";
    templateData: Record<string, string | number>;
    attachments?: NotificationEmailAttachment[];
}

export interface NotificationEmailAttachment {
    attachmentUrl: string;
    attachmentName: string;
    attachmentType: string;
}