output "lambda_send_email_arn" {
  value = aws_lambda_function.send_email_cognito.arn
}

output "notification_event_bus_name" {
  value = aws_cloudwatch_event_bus.notifications.name
}

output "notification_event_bus_arn" {
  value = aws_cloudwatch_event_bus.notifications.arn
}

