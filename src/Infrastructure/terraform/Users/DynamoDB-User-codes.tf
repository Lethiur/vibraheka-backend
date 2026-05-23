resource "aws_dynamodb_table" "Verification_Codes" {
  name         = "${local.table_prefix}VerificationCodes"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "Code"
  
  attribute {
    name = "Code"
    type = "S"
  }

  attribute {
    name = "UserEmail"
    type = "S"
  }

  global_secondary_index {
    name            = "UserEmail-Index"
    hash_key        = "UserEmail"
    projection_type = "ALL"
  }

  ttl {
    attribute_name = "ExpiresAtUnix"
    enabled        = true
  }
  
  tags = local.tags
}

output "dynamodb_users_codes_verification_codes_table_name" {
  value = aws_dynamodb_table.vibraheka-dynamodb-users-codes.name
}
