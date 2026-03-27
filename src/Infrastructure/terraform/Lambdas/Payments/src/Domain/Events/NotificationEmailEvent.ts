export default interface NotificationEmailEventDetail {
    recipient: string;
    subject: string;
    templateType: "subscription_thank_you" | "trial_ending_soon";
    templateData: Record<string, string | number>;
    attachments?: NotificationEmailAttachment[];
}

export interface NotificationEmailAttachment {
    attachmentUrl: string;
    attachmentName: string;
    attachmentType: string;
}