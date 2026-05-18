resource "aws_dynamodb_table" "VibraHeka_Catalog_SellableItems" {
  name = "VibraHeka-Catalog-SellableItems-${terraform.workspace}"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "SellableItemID"
  
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
}

output "DynamoDB_Catalog_SellableItem_TableName" {
  value = aws_dynamodb_table.VibraHeka_Catalog_SellableItems.name
  description = "The name of the DynamoDB table for Catalog Sellable Items"
}