resource "aws_dynamodb_table" "vibraheka-dynamodb-subscriptions" {
  name         = "VibraHeka-subscriptions-${terraform.workspace}"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "SubscriptionID"

  tags = {
    created : "terraform",
    environment : terraform.workspace,
    system: "VibraHeka",
    service : "Payments",
    dev : terraform.workspace != "prod"
  }
  
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
    projection_type    = "INCLUDE"
    non_key_attributes = ["SubscriptionID", "StartDate", "EndDate", "Status", "SubscriptionStatus", "ExternalSubscriptionID"]
  }
}
