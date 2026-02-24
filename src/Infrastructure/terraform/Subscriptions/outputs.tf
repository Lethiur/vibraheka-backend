output "dynamodb_subscription_table_name" {
  value       = aws_dynamodb_table.vibraheka-dynamodb-subscriptions.name                                          # The actual value to be outputted
  description = "The public IP address of the EC2 instance" # Description of what this output represents
}

output "dynamodb_subscription_table_arn" {
  value = aws_dynamodb_table.vibraheka-dynamodb-subscriptions.arn
  description = "The ARN of the subscriptions table"
}

output "subscriptions_user_id_index_name" {
  value = one([
    for gsi in aws_dynamodb_table.vibraheka-dynamodb-subscriptions.global_secondary_index : gsi.name
    if gsi.hash_key == "UserID"
  ])
}