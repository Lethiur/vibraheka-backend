
module "Config" {
  source        = "./Config"
  ssm_namespace = terraform.workspace == "default" ? var.project_name : "${var.project_name}/${terraform.workspace}"
}

module "Emails" {
  source = "./Emails"
}

module "Users" {
  source                            = "./Users"
  lambda_save_verification_code_arn = module.Lambda.lambda_save_verification_code_arn
  lambda_send_email_arn             = module.Lambda.lambda_send_email_arn
  prod_deployment                   = var.prod_deployment
}

module "Catalog" {
  source                      = "./Catalog"
}

module "Dev" {
  source = "./Dev"
}

module "Orders" {
  source = "./Orders"
}

module "Lambda" {
  source                                                = "./Lambdas"
  s3_templates_arn                                      = module.Emails.s3_email_templates_bucket_arn
  s3_templates_name                                     = module.Emails.s3_email_templates_bucket_name
  ses_domain_arn                                        = module.Emails.ses_email_domain_arn
  ses_config_arn                                        = module.Emails.ses_config_arn
  ses_config_name                                       = module.Emails.ses_config_name
  ses_from_email                                        = module.Emails.ses_from_email
  ssm_email_verification_template_id_parameter_name     = module.Config.ssm_email_verification_template_id_parameter_name
  ssm_user_welcome_template_param                       = module.Config.ssm_user_welcome_tempalte_id_parameter_name
  ssm_email_password_reset_template_id_parameter_name   = module.Config.ssm_email_password_reset_template_id_parameter_name
  ssm_subscription_thank_you_template_id_parameter_name = module.Config.ssm_subscription_thank_you_template_id_parameter_name
  ssm_subscription_cancelled_template_param             = module.Config.ssm_subscription_cancelled_template_id_parameter_name
  ssm_subscription_reactivated_template_param           = module.Config.ssm_subscription_reactivated_template_id_parameter_name
  ssm_trial_ending_soon_template_id_parameter_name      = module.Config.ssm_trial_ending_soon_template_id_parameter_name
  ssm_forgot_password_completed_template_param          = module.Config.ssm_forgot_password_completed_template_parameter_name
  kms_users_arn                                         = module.Users.kms_users_arn
  kms_users_key_alias_arn                               = module.Users.kms_users_key_alias_arn
  kms_users_key_alias_name                              = module.Users.kms_users_key_alias_name
  cognito_user_pool_arn                                 = module.Users.cognito_pool_users_arn
  dynamodb_codes_table_arn                              = module.Dev.dynamodb_table_codes_arn
  dynamodb_codes_table_name                             = module.Dev.dynamodb_table_codes_name
  ssm_read_parameters_policy_arn                        = module.Config.ssm_read_vh_parameters_policy_arn
  stripe_event_bus_arn                                  = var.stripe_event_bus_arn
  stripe_secret_key                                     = var.stripe_api_key
  password_reset_token_secret                           = var.password_reset_token_secret
  password_reset_frontend_url                           = "${var.ssm_namespace}${terraform.workspace}/frontend/url"
  password_reset_token_ttl_minutes                      = var.password_reset_token_ttl_minutes
  dynamodb_subscription_table                           = module.Subscriptions.dynamodb_subscription_table_name
  dynamodb_subscription_table_arn                       = module.Subscriptions.dynamodb_subscription_table_arn
}


module "ActionLog" {
  source = "./ActionLog"
}

module "Recordings" {
  source = "./Recordings"
}

module "Subscriptions" {
  source = "./Subscriptions"
}

module "BackendApi" {
  count                   = var.prod_deployment ? 1 : 0
  source                  = "./BackendApi"
  ssm_namespace           = terraform.workspace == "default" ? var.project_name : "${var.project_name}/${terraform.workspace}"
  ecr_repository_name     = "${var.project_name}-${terraform.workspace}"
  enable_ssh_ingress      = var.ec2_enable_ssh_ingress
  backend_instance_type   = var.ec2_instance_type
  backend_port            = var.ec2_backend_port
  existing_ssh_public_key = var.ec2_existing_ssh_public_key
  private_subnet_a_cidr   = var.private_subnet_a_cidr
  ssh_allowed_cidrs       = var.ec2_allowed_ciders
  vpc_cidr                = var.vpc_cidr
}
