
resource "aws_dynamodb_table" "Email_Templates" {
  name         = "${local.table_prefix}Templates"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "ID"

  attribute {
    name = "ID"
    type = "S"
  }
  
  restore_to_latest_time = true

  point_in_time_recovery {
    enabled = true
  }

  tags = merge(local.tags, {
    Component = "DynamoDB",
  })
  tags_all = merge(local.tags, {
    Component = "DynamoDB",
  })

}


output "dynamodb_email_templates_table_name" {
  value = aws_dynamodb_table.Email_Templates.name
}
