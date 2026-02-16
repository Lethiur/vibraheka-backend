variable "stripe_secret_key" {
  type        = string
  description = "Stripe Secret Key"
}

variable "stripe_event_bus_arn" {
  
}

variable "subscription_db_table_name" {
  default = ""
}

variable "dynamodb_table_arn" {}