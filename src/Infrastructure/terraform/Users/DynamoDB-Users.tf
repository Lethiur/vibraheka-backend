resource "aws_dynamodb_table" "vibraheka-dynamodb-users" {
  name         = "VibraHeka-users-${terraform.workspace}"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "Id"
  
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
  
  point_in_time_recovery {
    enabled = true
  }

  tags = local.tags
}

resource "aws_dynamodb_table" "Users_Profile" {
  name         = "${local.table_prefix}Profiles"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "Id"
  
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

  restore_source_name    = aws_dynamodb_table.vibraheka-dynamodb-users.name
  restore_to_latest_time = true
  
  tags = local.tags
}

output "dynamodb_users_profile_table_name" {
  value = aws_dynamodb_table.vibraheka-dynamodb-users.name
}
