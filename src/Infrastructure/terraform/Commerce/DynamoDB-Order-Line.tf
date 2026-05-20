
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

  tags = {
    created : "terraform",
    environment : terraform.workspace,
    system : "VibraHeka",
    service : "Orders",
    dev : terraform.workspace != "prod"
  }
}

output "order_lines_table_name" {
  value = aws_dynamodb_table.VH_order_lines.name
}
