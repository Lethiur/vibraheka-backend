

resource "aws_dynamodb_table" "Events_Attendees" {
  name         = "${local.table_prefix}Attendees"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "AttendeeID"

  attribute {
    name = "AttendeeID"
    type = "S"
  }

  attribute {
    name = "UserID"
    type = "S"
  }
  
  attribute {
    name = "EventID"
    type = "S"
  }
  
  global_secondary_index {
    hash_key        = "UserID"
    name            = "UserID-Index"
    projection_type = "ALL"
  }
  
  global_secondary_index {
    hash_key        = "EventID"
    name            = "EventID-Index"
    projection_type = "ALL"
  }

  tags = local.tags
}
