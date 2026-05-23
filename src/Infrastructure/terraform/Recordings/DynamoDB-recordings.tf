resource "aws_dynamodb_table" "Recordings_Records" {
  name         = "${local.table_prefix}Records"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "Id"

  attribute {
    name = "Id"
    type = "S"
  }

  attribute {
    name = "Tier"
    type = "S"
  }

  global_secondary_index {
    hash_key        = "Tier"
    name            = "tier-index"
    projection_type = "ALL"
  }

  point_in_time_recovery {
    enabled = true
  }

  restore_to_latest_time = true

  tags = merge(local.tags, {
    Component = "DynamoDB",
  })
  tags_all = merge(local.tags, {
    Component = "DynamoDB",
  })
}

output "dynamodb_recordings_records_table_name" {
  value = aws_dynamodb_table.Recordings_Records.name
}

output "dynamodb_recordings_records_table_arn" {
  value = aws_dynamodb_table.Recordings_Records.arn
}

output "dynamodb_recordings_table_tier_idx" {
  value = one([
    for gsi in aws_dynamodb_table.Recordings_Records.global_secondary_index : gsi.name
    if gsi.hash_key == "Tier"
  ])
}
