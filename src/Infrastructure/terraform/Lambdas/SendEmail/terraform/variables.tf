variable template_bucket_arn {}

variable template_bucket_name {}

variable ses_config_set_arn {}

variable ses_email_from {}

variable ssm_verification_template_param {}

variable ssm_password_reset_template_param {}

variable ssm_subscription_thank_you_template_param {}

variable ssm_subscription_cancelled_template_param {}

variable ssm_subscription_reactivated_template_param {}

variable ssm_user_welcome_template_param {}

variable ssm_trial_ending_soon_template_param {}

variable password_reset_token_secret {}

variable ssm_forgot_password_completed_template_param {}

variable password_reset_frontend_url {
  default = ""
}
variable "user_pool_arn" {
  default = ""
}
variable password_reset_token_ttl_minutes {
  default = 15
}

variable kms_arn {}

variable kms_alias_arn {}

variable "ses-domain-arn" {
}

variable "ses_config_set_name" {}

variable "kms_alias_name" {
}

variable "ssm_read_parameter_policy_arn" {}

variable "ses_contact_list_name" {}

variable "ses_contact_list_arn" {}
