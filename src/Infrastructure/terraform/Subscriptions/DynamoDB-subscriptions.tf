resource "aws_dynamodb_table" "vibraheka-dynamodb-subscriptions" {
  name         = "VibraHeka-subscriptions-${terraform.workspace}"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "SubscriptionID"
  
  attribute {
    name = "SubscriptionID"
    type = "S"
  }

  attribute {
    name = "UserID"
    type = "S"
  }
  
  attribute {
    name = "ExternalCustomerID"
    type = "S"
  }

  global_secondary_index {
    hash_key        = "ExternalCustomerID"
    name            = "ExternalCustomer-Index"
    projection_type = "ALL"
  }

  point_in_time_recovery {
    enabled = true
  }
  
  global_secondary_index {
    name               = "User-Index"
    hash_key           = "UserID"
    projection_type    = "ALL"
  }
  
  tags = local.tags
}

resource "aws_dynamodb_table" "Subscription_Records" {
  name         = "${local.table_prefix}Records"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "SubscriptionID"

  tags = local.tags

  attribute {
    name = "SubscriptionID"
    type = "S"
  }

  attribute {
    name = "UserID"
    type = "S"
  }

  attribute {
    name = "ExternalCustomerID"
    type = "S"
  }

  global_secondary_index {
    hash_key        = "ExternalCustomerID"
    name            = "ExternalCustomer-Index"
    projection_type = "ALL"
  }


  global_secondary_index {
    name               = "User-Index"
    hash_key           = "UserID"
    projection_type    = "ALL"
  }

  point_in_time_recovery {
    enabled = true
  }
  restore_source_name    = aws_dynamodb_table.vibraheka-dynamodb-subscriptions.name
  restore_to_latest_time = true
}


