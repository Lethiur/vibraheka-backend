output "cognito_pool_id" {
  value = module.Users.cognito_pool_user_id
}

output "cognito_client_id" {
  value = module.Users.cognito_user_pool_client_id
}

output "users_table_name" {
  value = module.Users.dynamodb-users-name
}

output "email_templates_bucket_name" {
  value = module.Emails.s3_email_templates_bucket_name
}

output "email_templates_table_name" {
  value = module.Emails.email_templates_table_name
}

output "verification_codes_table_name" {
  value = module.Dev.dynamodb_table_codes_name
}

output "action_log_table_name" {
  value = module.ActionLog.action_log_table_name
}

output "subscriptions_table_name" {
  value = module.Subscriptions.dynamodb_subscription_table_name
}

output "subscriptions_table_user_index_name" {
  value = module.Subscriptions.subscriptions_user_id_index_name
}

output "backend_api_gateway_endpoint" {
  value = module.BackendApi.api_gateway_endpoint
}

output "backend_api_gateway_base_route" {
  value = module.BackendApi.api_gateway_base_route
}

output "backend_api_gateway_id" {
  value = module.BackendApi.api_gateway_id
}

output "backend_ec2_instance_id" {
  value = module.BackendApi.backend_instance_id
}

output "backend_ec2_private_ip" {
  value = module.BackendApi.backend_instance_private_ip
}

output "backend_ec2_key_pair_name" {
  value = module.BackendApi.backend_instance_key_pair_name
}

output "backend_ec2_ssh_private_key_ssm_parameter_name" {
  value = module.BackendApi.backend_ssh_private_key_ssm_parameter_name
}

output "backend_ecr_repository_url" {
  value = module.BackendApi.ecr_repository_url
}

output "backend_ecr_repository_name" {
  value = module.BackendApi.ecr_repository_name
}

output "settings_namespace" {
  value = module.Config.settings_namespace
}


