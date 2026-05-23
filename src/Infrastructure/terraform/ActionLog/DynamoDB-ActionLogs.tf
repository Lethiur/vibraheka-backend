resource "aws_dynamodb_table" "ActionLogs_Records" {
  name         = "${local.table_prefix}Records"
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

  tags = local.tags
}

output "dynamodb_action_log_records_table_name" {
  value = aws_dynamodb_table.ActionLogs_Records.name
}

