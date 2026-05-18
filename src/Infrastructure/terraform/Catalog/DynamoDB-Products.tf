resource "aws_dynamodb_table" "VibraHeka_Catalog_Products" {
  name = "VibraHeka-Catalog-Products-${terraform.workspace}"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "ProductID"
  
  attribute {
    name = "ProductID"
    type = "S"
  }
}

output "DynamoDB_Catalog_Product_TableName" {
  value = aws_dynamodb_table.VibraHeka_Catalog_Products.name
  description = "The name of the DynamoDB table for Catalog Products"
}