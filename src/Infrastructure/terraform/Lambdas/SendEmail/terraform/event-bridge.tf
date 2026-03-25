resource "aws_cloudwatch_event_bus" "notifications" {
  name = "vibraheka-notifications-${terraform.workspace}"
}

resource "aws_cloudwatch_event_rule" "notification_email_requested" {
  name           = "notification-email-requested-${terraform.workspace}"
  event_bus_name = aws_cloudwatch_event_bus.notifications.name

  event_pattern = jsonencode({
    source = ["vibraheka.payments"]
    "detail-type" = ["email.notification.requested"]
  })
}

resource "aws_cloudwatch_event_target" "notification_email_lambda_target" {
  rule           = aws_cloudwatch_event_rule.notification_email_requested.name
  event_bus_name = aws_cloudwatch_event_bus.notifications.name
  target_id      = "sendEmailNotificationsLambda"
  arn            = aws_lambda_function.send_email_notifications.arn
}
