
resource "aws_dynamodb_table" "Events_Records" {
  name         = "${local.table_prefix}Records"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "EventID"

  attribute {
    name = "EventID"
    type = "S"
  }

  attribute {
    name = "EventDateUtc"
    type = "S"
  }

  attribute {
    name = "EventTimezone"
    type = "S"
  }

  global_secondary_index {
    name            = "EventsByDate-Index"
    hash_key        = "EventDateUtc"
    range_key       = "EventTimezone"
    projection_type = "ALL"
  }

  tags = local.tags
}
