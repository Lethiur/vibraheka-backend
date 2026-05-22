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

  point_in_time_recovery {
    enabled = true
  }
  
  tags = local.tags
}

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

  point_in_time_recovery {
    enabled = true
  }

  restore_source_name    = aws_dynamodb_table.vibraheka-dynamodb-users-action-log.name
  restore_to_latest_time = true

  tags = local.tags
}

output "dynamodb_action_log_records_table_name" {
  value = aws_dynamodb_table.vibraheka-dynamodb-users-action-log.name
}

