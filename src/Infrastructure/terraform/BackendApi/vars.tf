variable "context" {
  type = object({
    common_tags = map(string)
    project_name = string
    workspace = string
    resource_prefix = string
  })
}

locals {
  module_name = "Infrastructure"
  module_tags = {
    Module = local.module_name
  }

  tags = merge(var.context.common_tags, local.module_tags)
}

variable "vpc_cidr" {
  description = "CIDR block for the private VPC hosting backend resources."
  type        = string
}

variable "private_subnet_a_cidr" {
  description = "CIDR block for private subnet A."
  type        = string
}

variable "backend_instance_type" {
  description = "EC2 instance type for the backend host (spot)."
  type        = string
}

variable "backend_port" {
  description = "Port where the backend process listens inside EC2."
  type        = number
}

variable "ssh_allowed_cidrs" {
  description = "CIDR ranges allowed to connect via SSH to the EC2 instance."
  type        = list(string)
}

variable "enable_ssh_ingress" {
  description = "If true, opens inbound SSH (22) from ssh_allowed_cidrs. Prefer false and use SSM port forwarding instead."
  type        = bool
}

variable "existing_ssh_public_key" {
  description = "Optional existing SSH public key to use when create_ssh_key_pair is false."
  type        = string
}


variable "ecr_repository_name" {
  description = "Name for the ECR repository that stores backend container images."
  type        = string
}

variable "ssm_namespace" {
  description = "The namespace for the SSM parameters"
  type = string
}
