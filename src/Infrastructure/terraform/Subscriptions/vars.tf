variable "context" {
  type = object({
    common_tags          = map(string)
    resource_prefix      = string
  })
}

locals {
  module_name = "Subscriptions"
  module_tags = {
    Module    = local.module_name
    Persistent = "true"
    Component = "DynamoDB"
  }

  table_prefix = "${var.context.resource_prefix}${local.module_name}-"
  tags = merge(var.context.common_tags, local.module_tags)
}