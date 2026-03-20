export type NotificationTemplateType =
  | "subscription_thank_you"
  | "trial_ending_soon";

export interface NotificationEmailEventDetail {
  recipient: string;
  subject: string;
  templateType: NotificationTemplateType;
  templateData: Record<string, string | number>;
}
