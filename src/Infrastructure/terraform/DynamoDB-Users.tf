resource "aws_dynamodb_table" "vibraheka-dynamodb-users" {
  name         = "VibraHeka-users-${terraform.workspace}"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "Id"
  
  tags = {
    created : "terraform",
    environment : terraform.workspace,
    system: "VibraHeka",
    service : "PAM",
    dev : terraform.workspace != "prod"
  }


  
  attribute {
    name = "Id"
    type = "S"
  }

  attribute {
    name = "Email"
    type = "S"
  }

  attribute {
    name = "Role"
    type = "S"
  }

  global_secondary_index {
    name               = "EmailIndex"
    hash_key           = "Email"
    projection_type    = "INCLUDE"
    non_key_attributes = ["Id", "Email", "FirstName", "MiddleName", "Last Name", "Role", "TimezoneID"]
  }

  global_secondary_index {
    name               = "Role-Index"
    hash_key           = "Role"
    projection_type    = "INCLUDE"
    non_key_attributes =  ["Id", "Email", "FirstName", "MiddleName", "Last Name", "Role", "TimezoneID"]
  }
  
}
