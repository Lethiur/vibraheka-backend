
resource "aws_dynamodb_table" "VH_order_lines" {
  name         = "VibraHeka-Commerce-Order-Lines-${terraform.workspace}"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "OrderLineID"

  attribute {
    name = "OrderLineID"
    type = "S"
  }

  attribute {
    name = "OrderID"
    type = "S"
  }

  global_secondary_index {
    hash_key        = "OrderID"
    name            = "OrderID-Index"
    projection_type = "ALL"
  }
  point_in_time_recovery {
    enabled = true
  }


  tags = local.tags
}

resource "aws_dynamodb_table" "Commerce_Order_Lines" {
  name         = "${local.table_prefix}Order-Lines"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "OrderLineID"

  attribute {
    name = "OrderLineID"
    type = "S"
  }

  attribute {
    name = "OrderID"
    type = "S"
  }

  global_secondary_index {
    hash_key        = "OrderID"
    name            = "OrderID-Index"
    projection_type = "ALL"
  }

  restore_source_name    = aws_dynamodb_table.VH_order_lines.name
  restore_to_latest_time = true

  tags = local.tags
}

output "order_lines_table_name" {
  value = aws_dynamodb_table.VH_order_lines.name
}
