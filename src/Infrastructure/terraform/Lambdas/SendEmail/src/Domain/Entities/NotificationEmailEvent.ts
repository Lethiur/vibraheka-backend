export type NotificationTemplateType =
  | "subscription_thank_you"
  | "trial_ending_soon";

export default interface NotificationEmailEventDetail {
    recipient: string;
    subject: string;
    templateType: NotificationTemplateType;
    templateData: Record<string, string | number>;
    attachments?: NotificationEmailAttachment[];
}

export interface NotificationEmailAttachment {
    attachmentUrl: string;
    attachmentName: string;
    attachmentType: string;
}