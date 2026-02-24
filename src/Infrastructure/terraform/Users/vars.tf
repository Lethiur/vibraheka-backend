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
