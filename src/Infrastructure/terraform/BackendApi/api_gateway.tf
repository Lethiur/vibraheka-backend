# Internal NLB used as private entrypoint from API Gateway VPC Link to EC2.
resource "aws_lb" "backend_internal" {
  name               = local.lb_name
  internal           = true
  load_balancer_type = "network"
  subnets            = [aws_subnet.private_a.id, aws_subnet.private_b.id]

  tags = {
    Name        = local.lb_name
    environment = terraform.workspace
    created     = "terraform"
  }
}

# Target group for backend EC2 instance on application port.
resource "aws_lb_target_group" "backend" {
  name        = local.target_group_name
  port        = var.backend_port
  protocol    = "TCP"
  target_type = "instance"
  vpc_id      = aws_vpc.backend.id

  health_check {
    enabled  = true
    protocol = "TCP"
  }

  tags = {
    Name        = local.target_group_name
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

  cors_configuration {
    allow_origins = ["https://vh-049-loading-state-when-subscribing.d2h4h7jsyocr5v.amplifyapp.com"]
    allow_methods = ["GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS"]
    allow_headers = ["*"]
    expose_headers = ["*"]
    max_age        = 86400
  }
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

# Root route kept for simple health checks at '/'.
resource "aws_apigatewayv2_route" "root" {
  api_id    = aws_apigatewayv2_api.backend.id
  route_key = "ANY /"
  target    = "integrations/${aws_apigatewayv2_integration.backend_proxy.id}"
}

# Catch-all proxy route: every non-root path is forwarded to the same backend integration.
resource "aws_apigatewayv2_route" "proxy" {
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
