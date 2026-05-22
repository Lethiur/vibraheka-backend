
resource "aws_dynamodb_table" "VH_orders" {
  name         = "VibraHeka-Commerce-Orders-${terraform.workspace}"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "OrderID"

  attribute {
    name = "OrderID"
    type = "S"
  }
  
  attribute {
    name = "UserID"
    type = "S"
  }
  
  global_secondary_index {
    hash_key        = "UserID"
    name            = "user-index"
    projection_type = "ALL"
  }
  point_in_time_recovery {
    enabled = true
  }


  tags = local.tags
}


resource "aws_dynamodb_table" "Commerce_Orders" {
  name         = "${local.table_prefix}Orders"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "OrderID"

  attribute {
    name = "OrderID"
    type = "S"
  }

  attribute {
    name = "UserID"
    type = "S"
  }

  global_secondary_index {
    hash_key        = "UserID"
    name            = "user-index"
    projection_type = "ALL"
  }
  point_in_time_recovery {
    enabled = true
  }


  restore_source_name    = aws_dynamodb_table.VH_orders.name
  restore_to_latest_time = true
  tags = local.tags
}

output "orders_table_name" {
  value = aws_dynamodb_table.VH_orders.name
}
