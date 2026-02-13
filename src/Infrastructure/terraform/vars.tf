
variable ssm_namespace {
  default = "/VibraHeka/"
  description = "The SSM namespace for this environment"
}

variable prod_deployment {
  type = bool
  default = false
  description = "Whether we are performing a production deployment"
}

variable "stripe_event_bus_arn" {
  type = string
  description = "The ARN of the Stripe event bus"
}

variable "stripe_api_key" {
}
