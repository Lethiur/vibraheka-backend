resource "aws_dynamodb_table" "vibraheka-dynamodb-users-action-log" {
  name         = "VibraHeka-action-log-${terraform.workspace}"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "ActionLogID"
  range_key = "Action"
  
  attribute {
    name = "ActionLogID"
    type = "S"
  }

  attribute {
    name = "Action"
    type = "S"
  }
  
  tags = {
    created : "terraform",
    environment : terraform.workspace,
    system: "VibraHeka",
    service : "PAM",
    dev : terraform.workspace != "prod"
  }
}

output "action_log_table_name" {
  value = aws_dynamodb_table.vibraheka-dynamodb-users-action-log.name
}

