resource "aws_dynamodb_table" "vibraheka-dynamodb-users-codes" {
  name         = "VibraHeka-users-codes-${terraform.workspace}"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "Code"

  tags = {
    created : "terraform",
    environment : terraform.workspace,
    system: "VibraHeka",
    service : "PAM",
    dev : terraform.workspace != "prod"
  }
  
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
}

output "dynamodb-users-codes" {
  value = aws_dynamodb_table.vibraheka-dynamodb-users-codes.name
}
