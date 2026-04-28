resource "aws_iam_role_policy" "VH_ssm_read_parameters" {
  name = "ssm-read-parameters-policy-${terraform.workspace}"
  role = aws_iam_role.VH_email_lambda_role.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = [
          "ssm:GetParametersByPath",
          "ssm:GetParameter",
          "ssm:GetParameters"
        ]
        Effect   = "Allow"
        Resource = "arn:aws:ssm:*:*:parameter/VibraHeka/*"
      }
    ]
  })
}

locals {
  send_email_environment_variables = {
    TEMPLATE_BUCKET                                  = var.template_bucket_name
    SES_FROM_EMAIL                                   = var.ses_email_from
    SES_CONFIG_SET                                   = var.ses_config_set_name
    SSM_TEMPLATE_NAME_PARAM                          = var.ssm_verification_template_param // To remove
    SSM_VERIFICATION_TEMPLATE_NAME_PARAM             = var.ssm_verification_template_param
    SSM_PASSWORD_RESET_TEMPLATE_NAME_PARAM           = var.ssm_password_reset_template_param
    SSM_USER_WELCOME_TEMPLATE_NAME_PARAM             = var.ssm_user_welcome_template_param
    SSM_SUBSCRIPTION_THANK_YOU_TEMPLATE_NAME_PARAM   = var.ssm_subscription_thank_you_template_param
    SSM_SUBSCRIPTION_CANCELLED_TEMPLATE_NAME_PARAM   = var.ssm_subscription_cancelled_template_param
    SSM_SUBSCRIPTION_REACTIVATED_TEMPLATE_NAME_PARAM = var.ssm_subscription_reactivated_template_param
    SSM_FORGOT_PASSWORD_COMPLETED_TEMPLATE_NAME_PARAM = var.ssm_forgot_password_completed_template_param
    SSM_TRIAL_ENDING_SOON_TEMPLATE_NAME_PARAM        = var.ssm_trial_ending_soon_template_param
    SSM_PASSWORD_RESET_FRONTEND_URL                  = var.password_reset_frontend_url
    PASSWORD_RESET_TOKEN_SECRET                      = var.password_reset_token_secret
    PASSWORD_RESET_TOKEN_TTL_MINUTES                 = tostring(var.password_reset_token_ttl_minutes)
    KEY_ARN                                          = var.kms_arn
    KEY_ALIAS                                        = var.kms_alias_name
    AWS_NODEJS_CONNECTION_REUSE_ENABLED              = "1"
    SES_CONTACT_LIST_NAME                            = var.ses_contact_list_name
  }
}

