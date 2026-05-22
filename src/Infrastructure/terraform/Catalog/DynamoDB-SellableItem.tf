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
  
  point_in_time_recovery {
    enabled = true
  }

  tags = local.tags
 
}

resource "aws_dynamodb_table" "Catalog_SellableItems" {
  name         = "${local.table_prefix}SellableItems"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "SellableItemID"

  attribute {
    name = "SellableItemID"
    type = "S"
  }

  restore_source_name    = aws_dynamodb_table.VibraHeka_Catalog_SellableItems.name
  restore_to_latest_time = true

  point_in_time_recovery {
    enabled = true
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

output "DynamoDB_Catalog_SellableItem_TableName" {
  value = aws_dynamodb_table.Catalog_SellableItems.name
  description = "The name of the DynamoDB table for Catalog Sellable Items"
}