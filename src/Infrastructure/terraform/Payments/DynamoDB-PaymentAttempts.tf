resource "aws_dynamodb_table" "VibraHeka_Payment_Attempts" {
  name = "VibraHeka-Payment-Attempts-${terraform.workspace}"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "PaymentAttemptID"

  attribute {
    name = "PaymentAttemptID"
    type = "S"
  }
  
  attribute {
    name = "OrderId"
    type = "S"
  }
  
  attribute {
    name = "UserId"
    type = "S"
  }
  
  global_secondary_index {
    hash_key        = "OrderId"
    name            = "OrderId-Index"
    projection_type = "ALL"
  }
  
  global_secondary_index {
    hash_key        = "UserId"
    name            = "UserId-Index"
    projection_type = "ALL"
  }

  point_in_time_recovery {
    enabled = true
  }
}

resource "aws_dynamodb_table" "Payment_Attempts" {
  name = "${local.table_prefix}Attempts"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "PaymentAttemptID"

  attribute {
    name = "PaymentAttemptID"
    type = "S"
  }

  attribute {
    name = "OrderId"
    type = "S"
  }

  attribute {
    name = "UserId"
    type = "S"
  }

  global_secondary_index {
    hash_key        = "OrderId"
    name            = "OrderId-Index"
    projection_type = "ALL"
  }

  global_secondary_index {
    hash_key        = "UserId"
    name            = "UserId-Index"
    projection_type = "ALL"
  }
  
  restore_source_name    = aws_dynamodb_table.VibraHeka_Payment_Attempts.name
  restore_to_latest_time = true

  point_in_time_recovery {
    enabled = true
  }

  tags = local.tags
}

output "dynamodb_payment_attempts_table_name" {
  value = aws_dynamodb_table.VibraHeka_Payment_Attempts.name
  description = "The name of the DynamoDB table for Catalog Products"
}