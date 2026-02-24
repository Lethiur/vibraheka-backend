output "cognito_pool_id" {
  value = module.Users.cognito_pool_user_id
}

output "cognito_client_id" {
  value = module.Users.cognito_user_pool_client_id
}

output "users_table_name"  {
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


