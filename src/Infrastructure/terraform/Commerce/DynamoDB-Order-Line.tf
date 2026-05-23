
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
  
  tags = local.tags
}

output "dynamodb_commerce_order_lines_table_name" {
  value = aws_dynamodb_table.Commerce_Order_Lines.name
}
