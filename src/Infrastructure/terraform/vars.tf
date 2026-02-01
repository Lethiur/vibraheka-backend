
variable ssm_namespace {
  default = "/VibraHeka/"
  description = "The SSM namespace for this environment"
}

variable prod_deployment {
  type = bool
  default = false
  description = "Whether we are performing a production deployment"
}