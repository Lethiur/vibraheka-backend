output "cognito_pool_id" {
  value = module.Users.cognito_pool_user_id
}

output "cognito_client_id" {
  value = module.Users.cognito_user_pool_client_id
}

output "users_table_name" {
  value = module.Users.dynamodb-users-name
}

output "user_codes_table_name" {
  value = module.Users.dynamodb-users-codes
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
  value = try(module.BackendApi[0].api_gateway_endpoint, null)
}

output "backend_api_gateway_base_route" {
  value = try(module.BackendApi[0].api_gateway_base_route, null)
}

output "backend_api_gateway_id" {
  value = try(module.BackendApi[0].api_gateway_id, null)
}

output "backend_ec2_instance_id" {
  value = try(module.BackendApi[0].backend_instance_id, null)
}

output "backend_ec2_private_ip" {
  value = try(module.BackendApi[0].backend_instance_private_ip, null)
}

output "backend_ec2_public_ip" {
  value = try(module.BackendApi[0].backend_instance_public_ip, null)
}

output "backend_ec2_key_pair_name" {
  value = try(module.BackendApi[0].backend_instance_key_pair_name, null)
}

output "backend_ec2_ssh_private_key_ssm_parameter_name" {
  value = try(module.BackendApi[0].backend_ssh_private_key_ssm_parameter_name, null)
}

output "backend_ecr_repository_url" {
  value = try(module.BackendApi[0].ecr_repository_url, null)
}

output "backend_ecr_repository_name" {
  value = try(module.BackendApi[0].ecr_repository_name, null)
}

output "settings_namespace" {
  value = module.Config.settings_namespace
}


