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

  tags = local.tags
}

output "dynamodb_commerce_orders_table_name" {
  value = aws_dynamodb_table.Commerce_Orders.name
}
