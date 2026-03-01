resource "aws_dynamodb_table" "vibraheka-dynamodb-users-codes" {
  name         = "VibraHeka-users-codes-${terraform.workspace}"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "UserEmail"

  tags = {
    created : "terraform",
    environment : terraform.workspace,
    system: "VibraHeka",
    service : "PAM",
    dev : terraform.workspace != "prod"
  }
  
  attribute {
    name = "UserEmail"
    type = "S"
  }
}

output "dynamodb-users-codes" {
  value = aws_dynamodb_table.vibraheka-dynamodb-users-codes.name
}
