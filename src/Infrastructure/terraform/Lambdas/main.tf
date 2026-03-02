module "CreateChallengeLambda" {
  source                  = "./VerificationCode/terraform"
  dynamo_codes_table_arn  = var.dynamodb_codes_table_arn
  dynamo_codes_table_name = var.dynamodb_codes_table_name
  kms_alias_arn           = var.kms_users_key_alias_arn
  kms_alias_name          = var.kms_users_key_alias_name
  kms_arn                 = var.kms_users_arn
  user_pool_arn           = var.cognito_user_pool_arn
}

module "SendEmailLambda" {
  source                          = "./SendEmail/terraform"
  template_bucket_arn             = var.s3_templates_arn
  template_bucket_name            = var.s3_templates_name
  ses_config_set_arn              = var.ses_config_arn
  ses_config_set_name             = var.ses_config_name
  ses_email_from                  = var.ses_mail_from_domain
  ssm_verification_template_param = var.ssm_email_verification_template_id_parameter_name
  ssm_password_reset_template_param = var.ssm_email_password_reset_template_id_parameter_name
  password_reset_token_secret     = var.password_reset_token_secret
  password_reset_frontend_url     = var.password_reset_frontend_url
  password_reset_token_ttl_minutes = var.password_reset_token_ttl_minutes
  kms_alias_arn                   = var.kms_users_key_alias_arn
  kms_alias_name                  = var.kms_users_key_alias_name
  kms_arn                         = var.kms_users_arn
  user_pool_arn                   = var.cognito_user_pool_arn
  ses-domain-arn                  = var.ses_domain_arn
  ssm_read_parameter_policy_arn =  var.ssm_read_parameters_policy_arn
}

module "Payments" {
  source = "./Payments/terraform"
  stripe_secret_key = var.stripe_secret_key
  stripe_event_bus_arn = var.stripe_event_bus_arn
  subscription_db_table_name =  var.dynamodb_subscription_table
  dynamodb_table_arn = var.dynamodb_subscription_table_arn
}

output "lambda_save_verification_code_arn" {
  value = module.CreateChallengeLambda.lambda_save_verification_code_arn
}

output "lambda_send_email_arn" {
  value = module.SendEmailLambda.lambda_send_email_arn
}
