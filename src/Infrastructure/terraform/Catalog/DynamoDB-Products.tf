resource "aws_dynamodb_table" "Catalog_Products" {
  name         = "${local.table_prefix}Products"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "ProductID"

  attribute {
    name = "ProductID"
    type = "S"
  }
  
  tags = local.tags
}

output "dynamodb_catalog_products_table_name" {
  value       = aws_dynamodb_table.Catalog_Products.name
  description = "The name of the DynamoDB table for Catalog Products"
}
