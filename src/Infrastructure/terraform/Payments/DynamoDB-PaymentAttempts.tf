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
}

output "DynamoDB_Catalog_Product_TableName" {
  value = aws_dynamodb_table.VibraHeka_Payment_Attempts.name
  description = "The name of the DynamoDB table for Catalog Products"
}