resource "aws_dynamodb_table" "Catalog_SellableItemPrices" {
  name = "${local.table_prefix}SellableItemPrices"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "SellableItemPriceID"

  attribute {
    name = "SellableItemPriceID"
    type = "S"
  }
  attribute {
    name = "SellableItemID"
    type = "S"
  }  
  
  attribute {
    name = "Kind"
    type = "S"
  }

  global_secondary_index {
    hash_key        = "SellableItemID"
    name            = "SellableItemID-Index"
    projection_type = "ALL"
  }
  global_secondary_index {
    hash_key        = "SellableItemID"
    name            = "SellableItemID-Kind-Index"
    range_key       = "Kind"
    projection_type = "ALL"
  }

  tags = local.tags
}


output "dynamodb_catalog_sellable_item_prices_table_name" {
  value = aws_dynamodb_table.Catalog_SellableItemPrices.name
}