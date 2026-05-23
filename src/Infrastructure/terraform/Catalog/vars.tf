variable "context" {
  type = object({
    common_tags          = map(string)
    resource_prefix      = string
  })
}

locals {
  module_name = "Catalog"
  module_tags = {
    Module     = local.module_name
    Component  = "DynamoDB"
    Persistent = "true"
  }
  table_prefix = "${var.context.resource_prefix}${local.module_name}-"
  tags = merge(var.context.common_tags, local.module_tags)
}
