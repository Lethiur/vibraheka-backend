variable "ssm_namespace" {}

variable "context" {
  type = object({
    project_name    = string
    workspace       = string
    common_tags     = map(string)
    resource_prefix = string
  })
}

locals {
  module_name = "ActionLogs"
  module_tags = {
    Module     = local.module_name
    Component  = "SSM"
    Persistent = "true"
  }
  
  tags = merge(var.context.common_tags, local.module_tags)
}
