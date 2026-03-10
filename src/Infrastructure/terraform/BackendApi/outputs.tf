output "api_gateway_endpoint" {
  value       = aws_apigatewayv2_api.backend.api_endpoint
  description = "Public API Gateway endpoint that proxies all routes to the private backend."
}

output "api_gateway_base_route" {
  value       = "${aws_apigatewayv2_api.backend.api_endpoint}/"
  description = "Base API Gateway route URL (default stage)."
}

output "api_gateway_id" {
  value       = aws_apigatewayv2_api.backend.id
  description = "ID of the backend API Gateway HTTP API."
}

output "backend_instance_id" {
  value       = aws_instance.backend.id
  description = "Public EC2 instance id serving backend traffic."
}

output "backend_instance_private_ip" {
  value       = aws_instance.backend.private_ip
  description = "Private IP of the backend EC2 instance."
}

output "backend_instance_public_ip" {
  value       = aws_eip.backend.public_ip
  description = "Elastic IP of the backend EC2 instance."
}

output "backend_instance_key_pair_name" {
  value       = aws_key_pair.backend.key_name
  description = "SSH key pair name attached to backend EC2 instance."
}

output "backend_ssh_private_key_ssm_parameter_name" {
  value       = var.create_ssh_key_pair ? aws_ssm_parameter.backend_ssh_private_key[0].name : null
  description = "SSM SecureString parameter holding generated private SSH key for CI retrieval."
}

output "ecr_repository_url" {
  value       = aws_ecr_repository.backend.repository_url
  description = "ECR repository URL for backend container images."
}

output "ecr_repository_name" {
  value       = aws_ecr_repository.backend.name
  description = "ECR repository name for backend container images."
}
