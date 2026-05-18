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
}

output "DynamoDB_SellableItemPrice_TableName" {
  value = aws_dynamodb_table.VibraHeka_Catalog_SellableItemPrice.name
}