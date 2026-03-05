# Internal NLB used as private entrypoint from API Gateway VPC Link to EC2.
resource "aws_lb" "backend_internal" {
  name               = "vibraheka-be-nlb-${terraform.workspace}"
  internal           = true
  load_balancer_type = "network"
  subnets            = [aws_subnet.private_a.id, aws_subnet.private_b.id]

  tags = {
    Name        = "vibraheka-be-nlb-${terraform.workspace}"
    environment = terraform.workspace
    created     = "terraform"
  }
}

# Target group for backend EC2 instance on application port.
resource "aws_lb_target_group" "backend" {
  name        = "vibraheka-be-tg-${terraform.workspace}"
  port        = var.backend_port
  protocol    = "TCP"
  target_type = "instance"
  vpc_id      = aws_vpc.backend.id

  health_check {
    enabled  = true
    protocol = "TCP"
  }

  tags = {
    Name        = "vibraheka-be-tg-${terraform.workspace}"
    environment = terraform.workspace
    created     = "terraform"
  }
}

# Registers EC2 instance as target in backend target group.
resource "aws_lb_target_group_attachment" "backend_instance" {
  target_group_arn = aws_lb_target_group.backend.arn
  target_id        = aws_instance.backend_spot.id
  port             = var.backend_port
}

# NLB listener forwarding inbound TCP traffic to backend target group.
resource "aws_lb_listener" "backend" {
  load_balancer_arn = aws_lb.backend_internal.arn
  port              = var.backend_port
  protocol          = "TCP"

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.backend.arn
  }
}

# HTTP API Gateway exposed publicly.
resource "aws_apigatewayv2_api" "backend" {
  name          = "vibraheka-backend-api-${terraform.workspace}"
  protocol_type = "HTTP"
}

# Private VPC Link from API Gateway to internal NLB subnets.
resource "aws_apigatewayv2_vpc_link" "backend" {
  name               = "vibraheka-backend-vpc-link-${terraform.workspace}"
  security_group_ids = [aws_security_group.backend_instance.id]
  subnet_ids         = [aws_subnet.private_a.id, aws_subnet.private_b.id]
}

# Integration definition for all API routes to same backend target.
resource "aws_apigatewayv2_integration" "backend_proxy" {
  api_id                 = aws_apigatewayv2_api.backend.id
  integration_type       = "HTTP_PROXY"
  integration_method     = "ANY"
  connection_type        = "VPC_LINK"
  connection_id          = aws_apigatewayv2_vpc_link.backend.id
  integration_uri        = aws_lb_listener.backend.arn
  payload_format_version = "1.0"
  timeout_milliseconds   = 30000
}

# Explicit route mapping for known backend endpoints.
resource "aws_apigatewayv2_route" "explicit" {
  for_each = var.api_gateway_explicit_routes

  api_id    = aws_apigatewayv2_api.backend.id
  route_key = each.value
  target    = "integrations/${aws_apigatewayv2_integration.backend_proxy.id}"
}

# Root route kept for simple health checks at '/'.
resource "aws_apigatewayv2_route" "root" {
  api_id    = aws_apigatewayv2_api.backend.id
  route_key = "ANY /"
  target    = "integrations/${aws_apigatewayv2_integration.backend_proxy.id}"
}

# Optional catch-all proxy route to avoid breaking unknown future endpoints.
resource "aws_apigatewayv2_route" "proxy" {
  count = var.enable_proxy_fallback_route ? 1 : 0

  api_id    = aws_apigatewayv2_api.backend.id
  route_key = "ANY /{proxy+}"
  target    = "integrations/${aws_apigatewayv2_integration.backend_proxy.id}"
}

# Default stage with auto-deploy for route/integration changes.
resource "aws_apigatewayv2_stage" "default" {
  api_id      = aws_apigatewayv2_api.backend.id
  name        = "$default"
  auto_deploy = true
}
