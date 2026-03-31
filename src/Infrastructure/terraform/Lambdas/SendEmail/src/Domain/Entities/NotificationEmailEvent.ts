export type NotificationTemplateType =
  | "subscription_thank_you"
  | "trial_ending_soon"
  | "subscription_reactivated"
  | "subscription_cancelled";

export default interface NotificationEmailEventDetail {
    username: string;
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