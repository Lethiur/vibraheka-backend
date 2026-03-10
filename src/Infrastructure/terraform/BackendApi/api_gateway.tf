# HTTP API Gateway exposed publicly.
resource "aws_apigatewayv2_api" "backend" {
  name          = "vibraheka-backend-api-${terraform.workspace}"
  protocol_type = "HTTP"

  cors_configuration {
    allow_origins  = ["https://vh-049-loading-state-when-subscribing.d2h4h7jsyocr5v.amplifyapp.com"]
    allow_methods  = ["GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS"]
    allow_headers  = ["*"]
    expose_headers = ["*"]
    max_age        = 86400
  }
}

# Integration definition for all API routes to the public backend host.
resource "aws_apigatewayv2_integration" "backend_proxy" {
  api_id                 = aws_apigatewayv2_api.backend.id
  integration_type       = "HTTP_PROXY"
  integration_method     = "ANY"
  connection_type        = "INTERNET"
  integration_uri        = "http://${aws_eip.backend.public_ip}:${var.backend_port}"
  payload_format_version = "1.0"
  timeout_milliseconds   = 30000

  # Ensure API Gateway forwards the incoming request path to the backend.
  # Without this, API Gateway may call the integration URI path ("/") for every route,
  # causing 404s from the app for non-root endpoints.
  request_parameters = {
    "overwrite:path" = "$request.path"
  }
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
