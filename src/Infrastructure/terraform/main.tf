
module "Config" {
  source = "./Config"
  ssm_namespace = "VibraHeka"
}

module "Emails" {
  source = "./Emails"
}

module "Users" {
  source = "./Users"
  lambda_save_verification_code_arn = module.Lambda.lambda_save_verification_code_arn
  lambda_send_email_arn = module.Lambda.lambda_send_email_arn
  prod_deployment = var.prod_deployment
}

module "Dev" {
  source = "./Dev"
}

module "Lambda" {
  source = "./Lambdas"
  s3_templates_arn = module.Emails.s3_email_templates_bucket_arn
  s3_templates_name = module.Emails.s3_email_templates_bucket_name
  ses_domain_arn = module.Emails.ses_email_domain_arn
  ses_config_arn = module.Emails.ses_config_arn
  ses_config_name = module.Emails.ses_config_name
  ses_mail_from_domain = module.Emails.ses_email_from_domain
  ssm_email_verification_template_id_parameter_name = module.Config.ssm_email_verification_template_id_parameter_name
  kms_users_arn = module.Users.kms_users_arn
  kms_users_key_alias_arn = module.Users.kms_users_key_alias_arn
  kms_users_key_alias_name = module.Users.kms_users_key_alias_name
  cognito_user_pool_arn = module.Users.cognito_pool_users_arn
  dynamodb_codes_table_arn = module.Dev.dynamodb_table_codes_arn
  dynamodb_codes_table_name = module.Dev.dynamodb_table_codes_name
  ssm_read_parameters_policy_arn = module.Config.ssm_read_vh_parameters_policy_arn
}


module "ActionLog" {
  source = "./ActionLog"
}
