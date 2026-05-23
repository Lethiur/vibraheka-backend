resource "aws_dynamodb_table" "VibraHeka_Catalog_Products" {
  name         = "VibraHeka-Catalog-Products-${terraform.workspace}"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "ProductID"

  attribute {
    name = "ProductID"
    type = "S"
  }

  point_in_time_recovery {
    enabled = true
  }
}

resource "aws_dynamodb_table" "Catalog_Products" {
  name         = "${local.table_prefix}Products"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "ProductID"

  attribute {
    name = "ProductID"
    type = "S"
  }

  restore_source_name    = aws_dynamodb_table.VibraHeka_Catalog_Products.name
  restore_to_latest_time = true
  
  point_in_time_recovery {
    enabled = true
  }
  
  tags = local.tags
}

output "dynamodb_catalog_products_table_name" {
  value       = aws_dynamodb_table.Catalog_Products.name
  description = "The name of the DynamoDB table for Catalog Products"
}
