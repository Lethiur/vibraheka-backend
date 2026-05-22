variable "context" {
  type = object({
    common_tags          = map(string)
    resource_prefix      = string
  })
}

variable "prod_deployment" {
  type        = bool                     # The type of the variable, in this case a string
  default     = false                 # Default value for the variable
  description = "Whether to configure the deployment for production" # Description of what this variable represents
}

variable "lambda_send_email_arn" {
  type = string
  description = "The arn of the lambda in charge of sending emails"
}

variable "lambda_save_verification_code_arn" {
  type = string
  description = "The arn of the lambda in charge of saving the verification code in dynamo"
}

locals {
  module_name = "Users"
  module_tags = {
    Module    = local.module_name
    Persistent = "true"
    Component = "DynamoDB"
  }

  table_prefix = "${var.context.resource_prefix}${local.module_name}-"
  tags = merge(var.context.common_tags, local.module_tags)
}
