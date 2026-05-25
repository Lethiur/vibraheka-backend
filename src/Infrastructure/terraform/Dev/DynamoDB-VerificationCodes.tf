resource "aws_dynamodb_table" "Verification_Codes" {
  name         = "${local.table_prefix}CognitoCodes"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "username"
  

  attribute {
    name = "username"
    type = "S"
  }
  tags = local.tags
}

output "dynamodb_verification_codes_table_name" {
  value       = aws_dynamodb_table.Verification_Codes.name
  description = "The name of the terraform table that stores the codes. Only available in test environment"
}

output "dynamodb_verification_codes_table_arn" {
  value = aws_dynamodb_table.Verification_Codes.arn
}
