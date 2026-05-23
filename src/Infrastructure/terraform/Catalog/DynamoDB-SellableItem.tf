resource "aws_dynamodb_table" "Catalog_SellableItems" {
  name         = "${local.table_prefix}SellableItems"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "SellableItemID"

  attribute {
    name = "SellableItemID"
    type = "S"
  }

  attribute {
    name = "SellableItemID"
    type = "S"
  }

  attribute {
    name = "ReferenceID"
    type = "S"
  }

  global_secondary_index {
    hash_key        = "ReferenceID"
    name            = "ReferenceID-Index"
    projection_type = "ALL"
  }
  
  tags = local.tags
}

output "dynamodb_catalog_sellable_items_table_name" {
  value = aws_dynamodb_table.Catalog_SellableItems.name
  description = "The name of the DynamoDB table for Catalog Sellable Items"
}