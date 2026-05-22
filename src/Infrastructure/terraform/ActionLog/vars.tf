variable "context" {
  type = object({
    resource_prefix = string
    common_tags     = map(string)
  })
}

locals {
  module_name = "ActionLogs"
  module_tags = {
    Module    = local.module_name
    Component = "DynamoDB"
  }
  table_prefix = "${var.context.resource_prefix}${local.module_name}-"

  tags = merge(var.context.common_tags, local.module_tags)
}
