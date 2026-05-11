
resource "aws_dynamodb_table" "VH_orders" {
  name         = "VibraHeka-Orders-${terraform.workspace}"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "OrderID"

  attribute {
    name = "OrderID"
    type = "S"
  }
  
  attribute {
    name = "ProductID"
    type = "S"
  }
  
  attribute {
    name = "UserID"
    type = "S"
  }
  
  global_secondary_index {
    hash_key        = "ProductID"
    name            = "product-index"
    projection_type = "ALL"
  }
  
  global_secondary_index {
    hash_key        = "UserID"
    name            = "user-index"
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

output "orders_table_name" {
  value = aws_dynamodb_table.VH_orders.name
}
