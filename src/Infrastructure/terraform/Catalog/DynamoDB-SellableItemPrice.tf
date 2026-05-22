resource "aws_dynamodb_table" "VibraHeka_Catalog_SellableItemPrice" {
  name = "VibraHeka-Catalog-SellableItemPrice-${terraform.workspace}"
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
  
  global_secondary_index {
    hash_key        = "SellableItemID"
    name            = "SellableItemID-Index"
    projection_type = "ALL"
  }

  point_in_time_recovery {
    enabled = true
  }
}
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

  global_secondary_index {
    hash_key        = "SellableItemID"
    name            = "SellableItemID-Index"
    projection_type = "ALL"
  }

  restore_source_name    = aws_dynamodb_table.VibraHeka_Catalog_SellableItemPrice.name
  restore_to_latest_time = true
  
  point_in_time_recovery {
    enabled = true
  }

  tags = local.tags
}


output "DynamoDB_SellableItemPrice_TableName" {
  value = aws_dynamodb_table.Catalog_SellableItemPrices.name
}