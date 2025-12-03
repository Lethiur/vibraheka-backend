resource "aws_dynamodb_table" "vibraheka-dynamodb-users" {
  name         = "VibraHeka-users"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "Id"

  attribute {
    name = "Id"
    type = "S"
  }

  attribute {
    name = "Email"
    type = "S"
  }

  global_secondary_index {
    name               = "EmailIndex"
    hash_key           = "Email"
    projection_type    = "ALL"
  }
}
