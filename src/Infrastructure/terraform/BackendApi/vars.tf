variable "vpc_cidr" {
  description = "CIDR block for the private VPC hosting backend resources."
  type        = string
  default     = "10.60.0.0/16"
}

variable "private_subnet_a_cidr" {
  description = "CIDR block for private subnet A."
  type        = string
  default     = "10.60.1.0/24"
}

variable "private_subnet_b_cidr" {
  description = "CIDR block for private subnet B."
  type        = string
  default     = "10.60.2.0/24"
}

variable "backend_instance_type" {
  description = "EC2 instance type for the backend host (spot)."
  type        = string
  default     = "t4g.nano"
}

variable "backend_port" {
  description = "Port where the backend process listens inside EC2."
  type        = number
  default     = 5173
}

variable "ssh_allowed_cidrs" {
  description = "CIDR ranges allowed to connect via SSH to the EC2 instance."
  type        = list(string)
  default     = []
}

variable "create_ssh_key_pair" {
  description = "If true, Terraform creates an SSH keypair and stores private key in SSM SecureString."
  type        = bool
  default     = true
}

variable "existing_ssh_public_key" {
  description = "Optional existing SSH public key to use when create_ssh_key_pair is false."
  type        = string
  default     = ""

  validation {
    condition     = var.create_ssh_key_pair || trimspace(var.existing_ssh_public_key) != ""
    error_message = "When create_ssh_key_pair is false, existing_ssh_public_key must be provided."
  }
}

variable "ssh_private_key_ssm_parameter_name_prefix" {
  description = "SSM parameter prefix where generated private SSH key is stored."
  type        = string
  default     = "/VibraHeka/backend/ec2/ssh-private-key"
}

variable "api_gateway_explicit_routes" {
  description = "Explicit API Gateway route keys (HTTP API style) to map to backend integration."
  type        = set(string)
  default = [
    "GET /api/v1/admin/therapists",
    "PUT /api/v1/admin/addTherapist",
    "POST /api/v1/auth/register",
    "POST /api/v1/auth/authenticate",
    "GET /api/v1/auth/resend-confirmation-code",
    "POST /api/v1/auth/forgot-password",
    "POST /api/v1/auth/forgot-password/confirm",
    "POST /api/v1/auth/verification-code",
    "GET /api/v1/email-templates",
    "PUT /api/v1/email-templates/create",
    "PUT /api/v1/email-templates/add-attachment",
    "PUT /api/v1/email-templates/create-skeleton",
    "GET /api/v1/email-templates/url",
    "GET /api/v1/email-templates/contents",
    "GET /api/v1/settings/all-templates",
    "PUT /api/v1/subscriptions",
    "GET /api/v1/subscriptions",
    "GET /api/v1/subscriptions/details",
    "GET /api/v1/users/{id}",
  ]
}

variable "enable_proxy_fallback_route" {
  description = "If true, keep catch-all proxy route in API Gateway in addition to explicit routes."
  type        = bool
  default     = true
}

variable "ecr_repository_name" {
  description = "Name for the ECR repository that stores backend container images."
  type        = string
  default     = "vibraheka-backend"
}
